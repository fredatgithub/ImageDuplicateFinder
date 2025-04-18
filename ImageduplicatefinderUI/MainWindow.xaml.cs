using ImageduplicatefinderLib;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

namespace ImageduplicatefinderUI
{
  public partial class MainWindow : Window
  {
    private string _selectedDirectory;
    private ObservableCollection<DuplicateGroup> _duplicateGroups;
    private readonly ImageDuplicateFinder _imageDuplicateFinder;

    public MainWindow()
    {
      InitializeComponent();
      _imageDuplicateFinder = new ImageDuplicateFinder();
      _duplicateGroups = new ObservableCollection<DuplicateGroup>();
      lstDuplicateGroups.ItemsSource = _duplicateGroups;
      // Charger le dernier chemin utilisé
      _selectedDirectory = Properties.Settings.Default.LastDirectory;
      txtDirectory.Text = _selectedDirectory;

      // Charger la taille et la position de la fenêtre
if (Properties.Settings.Default.MainWindowWidth > 0)
    this.Width = Properties.Settings.Default.MainWindowWidth;
if (Properties.Settings.Default.MainWindowHeight > 0)
    this.Height = Properties.Settings.Default.MainWindowHeight;
if (Properties.Settings.Default.MainWindowTop >= 0)
    this.Top = Properties.Settings.Default.MainWindowTop;
if (Properties.Settings.Default.MainWindowLeft >= 0)
    this.Left = Properties.Settings.Default.MainWindowLeft;

    this.Closing += MainWindow_Closing;
    }

    private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
{
    Properties.Settings.Default.MainWindowWidth = this.Width;
    Properties.Settings.Default.MainWindowHeight = this.Height;
    Properties.Settings.Default.MainWindowTop = this.Top;
    Properties.Settings.Default.MainWindowLeft = this.Left;
    Properties.Settings.Default.Save();
}

    private void BtnSelectDirectory_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new FolderBrowserDialog();
      if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        _selectedDirectory = dialog.SelectedPath;
        txtDirectory.Text = _selectedDirectory;
      }
    }

    private async void BtnSearch_Click(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrEmpty(_selectedDirectory))
      {
        System.Windows.MessageBox.Show("Veuillez sélectionner un répertoire", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      PleaseWaitWindow pleaseWait = new PleaseWaitWindow
      {
        Owner = this
      };

      pleaseWait.Show();

      try
      {
        Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        btnSearch.IsEnabled = false;

        _duplicateGroups.Clear();
        var duplicates = await _imageDuplicateFinder.FindDuplicatesAsync(_selectedDirectory);

        foreach (var group in duplicates)
        {
          _duplicateGroups.Add(new DuplicateGroup
          {
            GroupName = $"Groupe {_duplicateGroups.Count + 1}",
            FileCount = group.Count(),
            Images = group.Select(f => new ImageInfo(f)).ToList()
          });
        }

        if (_duplicateGroups.Count == 0)
        {
          System.Windows.MessageBox.Show("Aucune image en double n'a été trouvée.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
      }
      catch (Exception exception)
      {
        System.Windows.MessageBox.Show($"Une erreur s'est produite : {exception.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
      }
      finally
      {
        Mouse.OverrideCursor = null;
        btnSearch.IsEnabled = true;
        pleaseWait.Close();
      }
    }

    private void LstDuplicateGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (lstDuplicateGroups.SelectedItem is DuplicateGroup group)
      {
        imgDuplicates.ItemsSource = group.Images;
      }
    }

    private void BtnOpenImage_Click(object sender, RoutedEventArgs e)
    {
      if (sender is System.Windows.Controls.Button button && button.CommandParameter is string filePath)
      {
        try
        {
          Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }
        catch (Exception exception)
        {
          System.Windows.MessageBox.Show($"Impossible d'ouvrir l'image : {exception.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      }
    }

    private void BtnDeleteImage_Click(object sender, RoutedEventArgs e)
    {
      if (sender is System.Windows.Controls.Button button && button.CommandParameter is string filePath)
      {
        // Recherche du groupe sélectionné
        if (lstDuplicateGroups.SelectedItem is DuplicateGroup group)
        {
          // Recherche de l'image à supprimer
          var imageToDelete = group.Images.FirstOrDefault(img => img.FilePath == filePath);
          if (imageToDelete != null)
          {
            // Suppression du fichier physique
            try
            {
              File.Delete(filePath);
            }
            catch (Exception ex)
            {
              System.Windows.MessageBox.Show($"Erreur lors de la suppression du fichier : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
              return;
            }

            // Suppression de l'image dans la liste du groupe
            group.Images.Remove(imageToDelete);
            group.FileCount = group.Images.Count;

            // Rafraîchir l'affichage
            imgDuplicates.ItemsSource = null;
            imgDuplicates.ItemsSource = group.Images;

            // Si le groupe n'a plus qu'une seule image, on supprime le groupe
            if (group.Images.Count == 1)
            {
              int currentIndex = _duplicateGroups.IndexOf(group);
              _duplicateGroups.Remove(group);

              // Sélectionner le groupe suivant si possible, sinon le précédent
              if (_duplicateGroups.Count > 0)
              {
                if (currentIndex < _duplicateGroups.Count)
                  lstDuplicateGroups.SelectedIndex = currentIndex;
                else
                  lstDuplicateGroups.SelectedIndex = _duplicateGroups.Count - 1;
              }
            }
          }
        }
      }
    }
    private void RefreshCurrentGroup()
    {
      if (lstDuplicateGroups.SelectedItem is DuplicateGroup group)
      {
        // Mettre à jour la liste des images
        group.Images.RemoveAll(img => !File.Exists(img.FilePath));
        group.FileCount = group.Images.Count;

        // Si le groupe ne contient plus qu'une seule image, le supprimer
        if (group.FileCount <= 1)
        {
          _duplicateGroups.Remove(group);
        }
        else
        {
          // Forcer le rafraîchissement de la vue
          imgDuplicates.ItemsSource = null;
          imgDuplicates.ItemsSource = group.Images;
        }
      }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
      Properties.Settings.Default.LastDirectory = _selectedDirectory;
      Properties.Settings.Default.Save();
      base.OnClosing(e);
    }
  }
}

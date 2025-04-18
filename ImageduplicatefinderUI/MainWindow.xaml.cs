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
        MessageBox.Show("Veuillez sélectionner un répertoire", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      try
      {
        Mouse.OverrideCursor = Cursors.Wait;
        btnSearch.IsEnabled = false;

        _duplicateGroups.Clear();
        var duplicates = await _imageDuplicateFinder.FindDuplicatesAsync(_selectedDirectory);

        foreach (var group in duplicates)
        {
          _duplicateGroups.Add(new DuplicateGroup
          {
            GroupName = $"Groupe {_duplicateGroups.Count + 1}",
            FileCount = group.Count(),
            Images = group.Select(f => new ImageInfo
            {
              FilePath = f,
              FileName = Path.GetFileName(f),
              FileSize = new FileInfo(f).Length
            }).ToList()
          });
        }

        if (_duplicateGroups.Count == 0)
        {
          MessageBox.Show("Aucune image en double n'a été trouvée.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
      }
      catch (Exception exception)
      {
        MessageBox.Show($"Une erreur s'est produite : {exception.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
      }
      finally
      {
        Mouse.OverrideCursor = null;
        btnSearch.IsEnabled = true;
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
      if (sender is Button button && button.CommandParameter is string filePath)
      {
        try
        {
          Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }
        catch (Exception exception)
        {
          MessageBox.Show($"Impossible d'ouvrir l'image : {exception.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      }
    }

    private void BtnDeleteImage_Click(object sender, RoutedEventArgs e)
    {
      if (sender is Button button && button.CommandParameter is string filePath)
      {
        var result = MessageBox.Show(
            $"Êtes-vous sûr de vouloir supprimer le fichier ?\n{filePath}",
            "Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
          try
          {
            File.Delete(filePath);
            RefreshCurrentGroup();
          }
          catch (Exception exception)
          {
            MessageBox.Show($"Impossible de supprimer le fichier : {exception.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
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
  }
}

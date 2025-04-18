using System;
using System.Windows.Media.Imaging;

namespace ImageduplicatefinderUI
{
  public class ImageInfo
  {
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public long FileSize { get; set; }

    public BitmapImage ImageSource
    {
      get
      {
        try
        {
          var bitmap = new BitmapImage();
          bitmap.BeginInit();
          bitmap.UriSource = new Uri(FilePath, UriKind.Absolute);
          bitmap.CacheOption = BitmapCacheOption.OnLoad; // charge en mémoire, ne verrouille plus le fichier
          bitmap.EndInit();
          bitmap.Freeze();
          return bitmap;
        }
        catch (Exception exception)
        {
          System.Diagnostics.Debug.WriteLine($"Erreur chargement image {FilePath} : {exception.Message}");
          return null;
        }
      }
    }
  }
}

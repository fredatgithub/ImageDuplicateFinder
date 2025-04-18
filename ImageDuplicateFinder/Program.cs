using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace ImageDuplicateFinder
{
  class Program
  {
    static void Main()
    {
      Action<string> Display = Console.WriteLine;
      const string folderPath = "F:\\repos\\SaveWindows10WallPaper\\SaveWindows10WallPaper\\images";
      Display($"Recherche des images en double dans le dossier : {folderPath}");
      if (!Directory.Exists(folderPath))
      {
        Display("Le dossier n'existe pas.");
        return;
      }

      string[] imageFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
      var supportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

      var hashToFileList = new Dictionary<string, List<string>>();

      foreach (var file in imageFiles)
      {
        if (!supportedExtensions.Contains(Path.GetExtension(file)))
        {
          continue;
        }

        try
        {
          string hash = ComputeImageHash(file);
          if (!hashToFileList.ContainsKey(hash))
          {
            hashToFileList[hash] = new List<string>();
          }

          hashToFileList[hash].Add(file);
          Display($"Traitée : {Path.GetFileName(file)}");
        }
        catch (Exception exception)
        {
          Display($"Erreur lors du traitement de {file}: {exception.Message}");
        }
      }

      Display("\n=== Images identiques détectées ===\n");
      foreach (var kvp in hashToFileList)
      {
        if (kvp.Value.Count > 1)
        {
          Display("Groupe de doublons:");
          foreach (var file in kvp.Value)
          {
            Display($" - {file}");
          }

          Display(string.Empty);
        }
      }

      Display("Analyse terminée. Appuyez sur une touche pour quitter.");
      Console.ReadKey();
    }

    static string ComputeImageHash(string filePath)
    {
      using (var md5 = MD5.Create())
      using (var stream = File.OpenRead(filePath))
      {
        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
      }
    }
  }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace ImageDuplicateFinder2
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
        Console.WriteLine("Le dossier n'existe pas.");
        return;
      }

      var imageFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
      var supportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

      var hashToFiles = new Dictionary<ulong, List<string>>();

      foreach (var file in imageFiles)
      {
        if (!supportedExtensions.Contains(Path.GetExtension(file)))
          continue;

        try
        {
          ulong hash = ComputeAverageHash(file);
          if (!hashToFiles.ContainsKey(hash))
            hashToFiles[hash] = new List<string>();

          hashToFiles[hash].Add(file);
          Console.WriteLine($"Traitée : {Path.GetFileName(file)}");
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Erreur avec {file}: {ex.Message}");
        }
      }

      var sb = new StringBuilder();
      sb.AppendLine("=== Images visuellement similaires ===");

      foreach (var group in hashToFiles.Where(g => g.Value.Count > 1))
      {
        sb.AppendLine("\nGroupe de doublons :");
        foreach (var file in group.Value)
        {
          sb.AppendLine($" - {file}");
          Console.WriteLine($" - {file}");
        }
      }

      string resultPath = Path.Combine(folderPath, "doublons_images.txt");
      File.WriteAllText(resultPath, sb.ToString());

      Console.WriteLine($"\nRésultats exportés vers : {resultPath}");
      Console.WriteLine("Appuyez sur une touche pour quitter.");
      Console.ReadKey();
    }

    static ulong ComputeAverageHash(string filePath)
    {
      using (var image = new Bitmap(filePath))
      {
        using (var resized = new Bitmap(8, 8))
        {
          using (var g = Graphics.FromImage(resized))
          {
            g.DrawImage(image, 0, 0, 8, 8);
          }

          ulong hash = 0;
          int total = 0;
          int[] grayValues = new int[64];

          for (int y = 0; y < 8; y++)
          {
            for (int x = 0; x < 8; x++)
            {
              Color pixel = resized.GetPixel(x, y);
              int gray = (pixel.R + pixel.G + pixel.B) / 3;
              grayValues[y * 8 + x] = gray;
              total += gray;
            }
          }

          int avg = total / 64;
          for (int i = 0; i < 64; i++)
          {
            if (grayValues[i] >= avg)
              hash |= 1UL << i;
          }

          return hash;
        }
      }
    }
  }
}

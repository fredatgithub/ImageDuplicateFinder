using System.Collections.Generic;

namespace ImageduplicatefinderUI
{
  public partial class MainWindow
  {
    public class DuplicateGroup
    {
      public string GroupName { get; set; }
      public int FileCount { get; set; }
      public List<ImageInfo> Images { get; set; } = new List<ImageInfo>();
    }

    public class ImageInfo
    {
      public string FilePath { get; set; }
      public string FileName { get; set; }
      public long FileSize { get; set; }
    }
  }
}
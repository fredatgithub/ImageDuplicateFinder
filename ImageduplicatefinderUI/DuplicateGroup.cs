using System.Collections.ObjectModel;

namespace ImageduplicatefinderUI
{
public partial class MainWindow
  {
    public class DuplicateGroup
    {
      public string GroupInfo { get; set; }
      public ObservableCollection<DuplicateImage> DuplicateFiles { get; set; }
    }

  }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace IVySoft.VDS.Client.UI.Logic.Files
{
    public interface IFileListProvider
    {
        string Path { get; set; }
        bool TryParsePath(string path);
        ObservableCollection<IFileListItem> Files { get; }

        System.Threading.Tasks.Task Refresh(System.Threading.CancellationToken token);
    }
}

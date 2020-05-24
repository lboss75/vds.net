using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace IVySoft.VDS.Client.UI.Logic.Files
{
    public interface IFileListFactory
    {
        ObservableCollection<IFileListSource> Sources { get; }
    }
}

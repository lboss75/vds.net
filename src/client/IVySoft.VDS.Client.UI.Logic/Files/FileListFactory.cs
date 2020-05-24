using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace IVySoft.VDS.Client.UI.Logic.Files
{
    public class FileListFactory : IFileListFactory
    {
        private readonly ObservableCollection<IFileListSource> sources_ = new ObservableCollection<IFileListSource>();

        public FileListFactory()
        {
            foreach(var disk in Environment.GetLogicalDrives())
            {
                this.sources_.Add(new LocalSystemFileListSource(disk));
            }
        }
        public ObservableCollection<IFileListSource> Sources => this.sources_;
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace IVySoft.VDS.Client.UI.Logic.Files
{
    internal class LocalSystemFileListProvider : IFileListProvider
    {
        private string path_;

        public LocalSystemFileListProvider(string root)
        {
            this.Path = root;
        }

        public string Path
        {
            get => this.path_;
            set
            {
                if(value != this.path_)
                {
                    var files = LoadFiles(value).ToList();
                    this.path_ = value;
                    this.Files.Clear();

                    foreach (var file in files)
                    {
                        this.Files.Add(file);
                    }
                }
            }
        }

        private IEnumerable<IFileListItem> LoadFiles(string value)
        {
            foreach(var dir in Directory.GetDirectories(value))
            {
                yield return new LocalSystemFileListItem(true, dir);
            }
            foreach (var f in Directory.GetFiles(value))
            {
                yield return new LocalSystemFileListItem(false, f);
            }
        }

        public bool TryParsePath(string path)
        {
            return (System.IO.Path.GetPathRoot(this.path_) == System.IO.Path.GetPathRoot(path));
        }

        public ObservableCollection<IFileListItem> Files { get; } = new ObservableCollection<IFileListItem>();
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;

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

                    CollectionUtils.Update(
                        this.Files,
                        files,
                        (x, y) => x.FullName == y.FullName,
                        (x, y) => ((LocalSystemFileListItem)x).Update(y),
                        (x) => x);

                    this.Files.Clear();
                }
            }
        }

        private IEnumerable<LocalSystemFileListItem> LoadFiles(string value)
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

        public async System.Threading.Tasks.Task Refresh(CancellationToken token)
        {
            var files = LoadFiles(this.path_).ToList();

            CollectionUtils.Update(
                this.Files,
                files,
                (x, y) => x.FullName == y.FullName,
                (x, y) => ((LocalSystemFileListItem)x).Update(y),
                (x) => x);

            this.Files.Clear();
        }

        public ObservableCollection<IFileListItem> Files { get; } = new ObservableCollection<IFileListItem>();
    }
}
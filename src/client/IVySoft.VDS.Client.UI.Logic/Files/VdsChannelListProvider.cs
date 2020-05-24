using System.Collections.ObjectModel;

namespace IVySoft.VDS.Client.UI.Logic.Files
{
    internal class VdsChannelListProvider : IFileListProvider
    {
        private readonly VdsChannelFileListSource owner_;
        private string path_;
        private ObservableCollection<IFileListItem> files_;

        public VdsChannelListProvider(VdsChannelFileListSource owner)
        {
            this.owner_ = owner;
        }

        public string Path
        {
            get => this.path_;
            set
            {
                if (this.path_ != value)
                {
                    this.files_ = this.owner_.GetFiles(value);
                    this.path_ = value;
                }
            }
        }

        public ObservableCollection<IFileListItem> Files => this.files_;

        public bool TryParsePath(string path)
        {
            return this.owner_.HaveFolder(path);
        }
    }
}
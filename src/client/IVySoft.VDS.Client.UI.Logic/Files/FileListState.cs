using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IVySoft.VDS.Client.UI.Logic.Files
{
    public class FileListState : INotifyPropertyChanged
    {
        private readonly IFileListFactory fileListFactory_;
        private IFileListSource fileListSource_;
        private IFileListProvider fileListProvider_;

        public FileListState(IFileListFactory fileListFactory)
        {
            this.fileListFactory_ = fileListFactory;
        }

        public ObservableCollection<IFileListSource> Sources { get => this.fileListFactory_.Sources; }
        public IFileListSource CurrentSource
        {
            get => this.fileListSource_;
            set
            {
                if(this.fileListSource_ != value)
                {
                    this.fileListSource_ = value;
                    if (value == null)
                    {
                        this.fileListProvider_ = null;
                    }
                    else
                    {
                        this.fileListProvider_ = value.CreateProvider();
                    }

                    if(this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(CurrentSource)));
                        this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(Path)));
                        this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(Files)));
                    }
                }
            }
         }

        public async Task Refresh(CancellationToken token, Action<Action> dispatchAction)
        {
            if(null != this.fileListProvider_)
            {
                await this.fileListProvider_.Refresh(token, dispatchAction);
            }
        }

        public string Path
        {
            get => this.fileListProvider_?.Path;
            set
            {
                if(value != this.fileListProvider_.Path)
                {
                    if(this.fileListProvider_ != null)
                    {
                        if (this.fileListProvider_.TryParsePath(value))
                        {
                            this.fileListProvider_.Path = value;
                            this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(Path)));
                            this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(Files)));
                            return;
                        }
                    }
                    foreach(var source in this.Sources)
                    {
                        var provider = source.CreateProvider();
                        if (provider.TryParsePath(value))
                        {
                            this.fileListProvider_ = provider;
                            this.fileListProvider_.Path = value;
                            this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(CurrentSource)));
                            this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(Path)));
                            this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(Files)));
                            return;
                        }
                    }
                    throw new Exception("Invalid path " + value);
                }
            }
        }
        public ObservableCollection<IFileListItem> Files { get => this.fileListProvider_?.Files; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

using IVySoft.VDS.Client.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace IVySoft.VDS.Client.UI.Logic.Files
{
    public class VdsChannelFileListSource : IFileListSource
    {
        public static readonly string VdsChannel = "VDS Channel";
        private readonly ThisUser user_;
        private readonly Channel channel_;
        private readonly Dictionary<string, ObservableCollection<IFileListItem>> files_ = new Dictionary<string, ObservableCollection<IFileListItem>>();

        public VdsChannelFileListSource(ThisUser user, Channel channel)
        {
            this.user_ = user;
            this.channel_ = channel;
        }

        internal ObservableCollection<IFileListItem> GetFiles(string path)
        {
            return this.files_[path];
        }

        internal bool HaveFolder(string path)
        {
            return this.files_.ContainsKey(path);
        }

        internal async System.Threading.Tasks.Task UpdateFiles(System.Threading.CancellationToken token)
        {
            using (var s = new VdsService())
            {
                foreach(var message in await s.Api.GetChannelMessages(token, this.channel_))
                {
                    foreach(var f in message.Files)
                    {
                        this.AddFile(f);
                    }
                }
            }
        }

        private void AddFile(ChannelMessageFileInfo f)
        {
            string path;
            string name;

            var pos = f.Name.LastIndexOf('/');
            if(pos < 0)
            {
                path = string.Empty;
                name = f.Name;
            }
            else
            {
                path = f.Name.Substring(0, pos);
                name = f.Name.Substring(pos + 1);
            }

            var files = this.CreateFolder(path);

            foreach(VdsChannelFileListItem file in files)
            {
                if(file.Name == name && !file.IsFolder)
                {
                    file.Merge(f);
                    return;
                }
            }

            files.Add(new VdsChannelFileListItem(f.Name, name, f));
        }
        private ObservableCollection<IFileListItem> CreateFolder(string file_path)
        {
            ObservableCollection<IFileListItem> files;
            if (!this.files_.TryGetValue(file_path, out files))
            {
                files = new ObservableCollection<IFileListItem>();
                this.files_.Add(file_path, files);
            }

            if (file_path.Length != 0)
            {
                string path;
                string name;

                var pos = file_path.LastIndexOf('/');
                if (pos < 0)
                {
                    path = string.Empty;
                    name = file_path;
                }
                else
                {
                    path = file_path.Substring(0, pos);
                    name = file_path.Substring(pos + 1);
                }

                var parent = this.CreateFolder(path);
                foreach (VdsChannelFileListItem file in parent)
                {
                    if (file.Name == name && file.IsFolder)
                    {
                        return files;
                    }
                }

                parent.Add(new VdsChannelFileListItem(file_path, name));
            }
            return files;
        }

        public string Kind => VdsChannel;

        public IFileListProvider CreateProvider()
        {
            return new VdsChannelListProvider(this);
        }
    }
}

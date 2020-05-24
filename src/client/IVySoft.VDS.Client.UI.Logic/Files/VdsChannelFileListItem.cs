using IVySoft.VDS.Client.Api;
using System;
using System.Collections.Generic;
using System.IO;

namespace IVySoft.VDS.Client.UI.Logic.Files
{
    internal class VdsChannelFileListItem : IFileListItem
    {
        private ChannelMessageFileInfo file_;
        private List<ChannelMessageFileInfo> history_ = new List<ChannelMessageFileInfo>();

        private readonly byte[] icon_;
        private readonly bool is_folder_;
        private readonly string name_;
        private readonly string full_name_;
        private readonly long size_;

        public VdsChannelFileListItem(string full_name, string name, ChannelMessageFileInfo file)
        {
            this.file_ = file;
            this.is_folder_ = false;
            this.name_ = name;
            this.full_name_ = full_name;
            this.size_ = file.Size;
        }
        public VdsChannelFileListItem(string full_name, string name)
        {
            this.is_folder_ = true;
            this.name_ = name;
            this.full_name_ = full_name;
            this.size_ = 0;

        }

        public byte[] Icon => this.icon_;
        public bool IsFolder => this.is_folder_;
        public string Name => this.name_;
        public string FullName => this.full_name_;
        public long Size => this.size_;

        internal void Merge(ChannelMessageFileInfo f)
        {
            this.history_.Add(f);
        }
    }
}
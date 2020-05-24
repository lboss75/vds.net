using IVySoft.VDS.Client.Transactions.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IVySoft.VDS.Client.Api
{
    public class ChannelMessageFileInfo
    {
        private readonly FileInfo data_;

        public byte[] Id { get => this.data_.Id; }
        public string Name { get => this.data_.Name; }
        public string MimeType { get => this.data_.MimeType; }
        public long Size { get => this.data_.Size; }
        public IEnumerable<ChannelMessageFileBlock> Blocks { get => this.data_.Blocks.Select(x => new ChannelMessageFileBlock(x)); }


        internal ChannelMessageFileInfo(FileInfo x)
        {
            this.data_ = x;
        }
    }
}
﻿using IVySoft.VDS.Client.Transactions.Data;

namespace IVySoft.VDS.Client.Api
{
    public class ChannelMessageFileBlock
    {
        private readonly FileBlock data_;

        internal FileBlock Data { get => this.data_; }
        public byte[] Id { get => this.data_.BlockId; }

        internal ChannelMessageFileBlock(FileBlock data)
        {
            this.data_ = data;
        }
    }
}
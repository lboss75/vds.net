using System;
using System.Collections.Generic;
using System.IO;

namespace IVySoft.VDS.Client.Transactions.Data
{
    internal class FileBlock
    {
        private byte[] block_id_ { get; set; }
        private byte[] block_key_ { get; set; }
        private long block_size_ { get; set; }

        public FileBlock(byte[] block_id, byte[] block_key, long block_size)
        {
            block_id_ = block_id;
            block_key_ = block_key;
            block_size_ = block_size;
        }

        public byte[] BlockId { get => block_id_; }
        public byte[] BlockKey { get => block_key_; }
        public long BlockSize { get => block_size_; }

        internal static FileBlock Deserialize(Stream stream)
        {
            var block_id = stream.pop_data();
            var block_key = stream.pop_data();
            var block_size = stream.get_int64();

            return new FileBlock(block_id, block_key, block_size);
        }

        internal void Serialize(Stream stream)
        {
            stream.push_data(block_id_);
            stream.push_data(block_key_);
            stream.push_int64(block_size_);
        }
    }
}
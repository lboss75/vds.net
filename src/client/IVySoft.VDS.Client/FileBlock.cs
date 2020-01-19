using System;
using System.Collections.Generic;
using System.IO;

namespace IVySoft.VDS.Client
{
    public class FileBlock
    {
        private byte[] block_id_ { get; set; }
        private byte[] block_key_ { get; set; }
        private Int64 block_size_ { get; set; }

        public FileBlock(byte[] block_id, byte[] block_key, Int64 block_size)
        {
            this.block_id_ = block_id;
            this.block_key_ = block_key;
            this.block_size_ = block_size;
        }

        public byte[] BlockId { get => block_id_; }
        public byte[] BlockKey { get => block_key_; }
        public Int64 BlockSize { get => block_size_; }

        internal static FileBlock Deserialize(Stream stream)
        {
            var block_id = stream.pop_data();
            var block_key = stream.pop_data();
            var block_size = stream.get_int64();

            return new FileBlock(block_id, block_key, block_size);
        }

        internal void Serialize(Stream stream)
        {
            stream.push_data(this.block_id_);
            stream.push_data(this.block_key_);
            stream.push_int64(this.block_size_);
        }
    }
}
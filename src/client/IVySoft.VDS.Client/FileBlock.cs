using System;
using System.Collections.Generic;
using System.IO;

namespace IVySoft.VDS.Client
{
    public class FileBlock
    {
        public FileBlock(byte[] block_id, byte[] block_key, byte[][] replica_hashes, Int64 block_size)
        {
            this.block_id_ = block_id;
            this.block_key_ = block_key;
            this.replica_hashes_ = replica_hashes;
            this.block_size_ = block_size;
        }

        private byte[] block_id_ { get; set; }
        private byte[] block_key_ { get; set; }
        private byte[][] replica_hashes_ { get; set; }
        private Int64 block_size_ { get; set; }

        public byte[] BlockId { get => block_id_; }
        public byte[] BlockKey { get => block_key_; }
        public byte[][] Replicas { get => replica_hashes_; }

        internal static FileBlock Deserialize(Stream stream)
        {
            var block_id = stream.pop_data();
            var block_key = stream.pop_data();

            var row_count = stream.read_number();
            var replica_hashes = new List<byte[]>();
            for (var i = 0; i < row_count; ++i)
            {
                replica_hashes.Add(stream.pop_data());
            }
            var block_size = stream.get_int64();

            return new FileBlock(block_id, block_key, replica_hashes.ToArray(), block_size);
        }
    }
}
namespace IVySoft.VDS.Client
{
    internal class FileBlock
    {
        public byte[] block_id { get; set; }
        public byte[] block_key { get; set; }
        public byte[][] replica_hashes { get; set; }
        public int block_size { get; set; }
    }
}
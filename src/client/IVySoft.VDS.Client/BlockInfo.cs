namespace IVySoft.VDS.Client
{
    internal class BlockInfo
    {
        public string[] replicas { get; set; }
        public string hash { get; set; }
        public System.Int32 replica_size { get; set; }
    }
}
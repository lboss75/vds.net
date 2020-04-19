namespace IVySoft.VDS.Client.Transactions.Data
{
    internal class BlockInfo
    {
        public string[] replicas { get; set; }
        public string hash { get; set; }
        public int replica_size { get; set; }
    }
}
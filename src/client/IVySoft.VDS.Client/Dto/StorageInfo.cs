namespace IVySoft.VDS.Client
{
    public class StorageInfo
    {
        public string id { get; set; }
        public string local_path { get; set; }
        public long reserved_size { get; set; }
        public long used_size { get; set; }
        public long free_size { get; set; }
    }
}
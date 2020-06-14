namespace IVySoft.VDS.Client.Api
{
    public class SyncStatistic
    {
        public string block { get; set; }
        public SyncStatisticReplica[] replicas { get; set; }
    }
}
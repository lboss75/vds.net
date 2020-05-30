namespace IVySoft.VDS.Client.Api
{
    public class SessionStatistic
    {
        public int send_queue_size { get; set; }

        public SessionStatisticRow[] items { get; set; }
    }
}
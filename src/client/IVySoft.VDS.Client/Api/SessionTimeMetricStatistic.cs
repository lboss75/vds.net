namespace IVySoft.VDS.Client.Api
{
    public class SessionTimeMetricStatistic
    {
        public long start { get; set; }
        public long finish { get; set; }
        public int mtu { get; set; }
        public int output_queue { get; set; }
        public int input_queue { get; set; }
        public int idle { get; set; }
        public int delay { get; set; }
        public int service { get; set; }

        public SessionTrafficStatistic[] traffic { get; set; }
    }
}
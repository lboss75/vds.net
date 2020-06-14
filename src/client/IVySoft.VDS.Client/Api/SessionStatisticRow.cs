namespace IVySoft.VDS.Client.Api
{
    public class SessionStatisticRow
    {
        public string partner { get; set; }
        public string address { get; set; }
        public bool blocked { get; set; }
        public bool not_started { get; set; }
        public SessionTimeMetricStatistic[] metrics { get; set; }
    }
}
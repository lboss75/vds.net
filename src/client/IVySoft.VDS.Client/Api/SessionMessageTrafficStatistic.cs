namespace IVySoft.VDS.Client.Api
{
    public class SessionMessageTrafficStatistic
    {
        public string msg { get; set; }
        public long sent { get; set; }
        public long sent_count { get; set; }
        public long rcv_good { get; set; }
        public long rcv_good_count { get; set; }
        public long rcv_bad { get; set; }
        public long rcv_bad_count { get; set; }
    }
}
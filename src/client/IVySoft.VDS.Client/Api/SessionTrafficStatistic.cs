namespace IVySoft.VDS.Client.Api
{
    public class SessionTrafficStatistic
    {
        public string from { get; set; }
        public SessionTrafficDestStatistic[] to { get; set; }
    }
}
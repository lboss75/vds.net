namespace IVySoft.VDS.Client.Api
{
    public class SessionTrafficDestStatistic
    {
        public string to { get; set; }
        public SessionMessageTrafficStatistic[] messages { get; set; }
    }
}
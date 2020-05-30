namespace IVySoft.VDS.Client.Api
{
    public class RouteStatisticItem
    {
        public string node_id { get; set; }
        public string proxy { get; set; }
        public long pinged { get; set; }
        public long hops { get; set; }
    }
}
namespace IVySoft.VDS.Client
{
    internal class WebsocketRequest
    {
        public int id { get; set; }
        public string invoke { get; set; }
        public object[] @params { get; set; }
    }
}
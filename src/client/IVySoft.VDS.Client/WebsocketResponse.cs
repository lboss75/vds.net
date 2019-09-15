using Newtonsoft.Json.Linq;

namespace IVySoft.VDS.Client
{
    internal class WebsocketResponse
    {
        public int id { get; set; }
        public JToken result { get; set; }
    }
}
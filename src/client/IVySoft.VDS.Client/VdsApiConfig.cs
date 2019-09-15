using System.Runtime.Serialization;

namespace IVySoft.VDS.Client
{
    public class VdsApiConfig
    {
        public string ServiceUri { get; set; }
        public int ConnectionTimeout { get; set; } = 30;
        public int SendTimeout { get; set; } = 30;
    }
}
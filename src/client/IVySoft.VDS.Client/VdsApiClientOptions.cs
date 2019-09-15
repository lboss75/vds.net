using System;

namespace IVySoft.VDS.Client
{
    public class VdsApiClientOptions
    {
        public Uri ServiceUri { get; set; }
        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan SendTimeout { get; set; } = TimeSpan.FromSeconds(30);
    }
}
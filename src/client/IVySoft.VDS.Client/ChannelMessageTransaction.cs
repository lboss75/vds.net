using System;

namespace IVySoft.VDS.Client
{
    internal class ChannelMessageTransaction
    {
        public string message { get; set; }
        public UploadedFile[] files { get; set; }

        internal byte[] Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
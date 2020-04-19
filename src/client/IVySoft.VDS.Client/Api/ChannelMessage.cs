using IVySoft.VDS.Client.Transactions;
using IVySoft.VDS.Client.Transactions.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVySoft.VDS.Client.Api
{
    public class ChannelMessage
    {
        private readonly UserMessageTransaction tr_;

        internal ChannelMessage(UserMessageTransaction transaction)
        {
            this.tr_ = transaction;
        }

        public string Message { get => this.tr_.Message; }
        public IEnumerable<ChannelMessageFileInfo> Files { get => this.tr_.Files.Select(x => new ChannelMessageFileInfo(x)); }
    }
}

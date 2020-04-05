using System;
using System.Collections.Generic;
using System.Text;

namespace IVySoft.VDS.Client.Dto
{
    internal class WalletBalanceMessage
    {
        public string issuer { get; set; }
        public string currency { get; set; }
        public long confirmed_balance { get; set; }
        public long proposed_balance { get; set; }
    }
}

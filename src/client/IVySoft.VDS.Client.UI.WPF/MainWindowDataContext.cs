using IVySoft.VDS.Client.Transactions;
using IVySoft.VDS.Client.UI.Logic.Model;
using System.Collections.ObjectModel;

namespace IVySoft.VDS.Client.UI.WPF
{
    internal class MainWindowDataContext
    {
        public ObservableCollection<Transactions.ChannelCreateTransaction> ChannelList { get; set; } = new ObservableCollection<Transactions.ChannelCreateTransaction>();
        public ObservableCollection<Logic.Model.ChannelMessage> MessagesList { get; set; } = new ObservableCollection<Logic.Model.ChannelMessage>();
        public ChannelCreateTransaction SelectedChannel { get; internal set; }
    }
}
using System.Collections.ObjectModel;

namespace IVySoft.VDS.Client.UI.WPF
{
    internal class MainWindowDataContext
    {
        public ObservableCollection<Transactions.ChannelCreateTransaction> ChannelList { get; set; } = new ObservableCollection<Transactions.ChannelCreateTransaction>();
        public ObservableCollection<Transactions.UserMessageTransaction> MessagesList { get; set; } = new ObservableCollection<Transactions.UserMessageTransaction>();
    }
}
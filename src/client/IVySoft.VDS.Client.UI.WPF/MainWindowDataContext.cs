using IVySoft.VDS.Client.Transactions;
using IVySoft.VDS.Client.UI.Logic.Model;
using System.Collections.ObjectModel;

namespace IVySoft.VDS.Client.UI.WPF
{
    internal class MainWindowDataContext
    {
        public ObservableCollection<Api.Channel> ChannelList { get; set; } = new ObservableCollection<Api.Channel>();
        public ObservableCollection<Logic.Model.ChannelMessage> MessagesList { get; set; } = new ObservableCollection<Logic.Model.ChannelMessage>();
        public Api.Channel SelectedChannel { get; internal set; }
    }
}
using System.Collections.ObjectModel;

namespace IVySoft.VDS.Client.UI.WPF.Monitor
{
    internal class StatisticsDataContext
    {
        public ObservableCollection<ServerStatisticRow> Servers { get; } = new ObservableCollection<ServerStatisticRow>();

        public StatisticsDataContext()
        {
        }
    }
}
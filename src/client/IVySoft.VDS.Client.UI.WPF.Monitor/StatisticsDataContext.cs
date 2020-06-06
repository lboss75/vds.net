using System.Collections.ObjectModel;
using System.ComponentModel;

namespace IVySoft.VDS.Client.UI.WPF.Monitor
{
    internal class StatisticsDataContext : INotifyPropertyChanged
    {
        private ServerStatisticRow selected_server_;
        private ServerSessionStatistic selected_session_;
        public ObservableCollection<ServerStatisticRow> Servers { get; } = new ObservableCollection<ServerStatisticRow>();
        

        public StatisticsDataContext()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ServerStatisticRow SelectedServer
        {
            get => this.selected_server_;
            set
            {
                if(this.selected_server_ != value)
                {
                    this.selected_server_ = value;
                    this.selected_session_ = null;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedServer)));
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSession)));
                }
            }
        }

        public ServerSessionStatistic SelectedSession
        {
            get => this.selected_session_;
            set
            {
                if (this.selected_session_ != value)
                {
                    this.selected_session_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSession)));
                }
            }
        }

    }
}
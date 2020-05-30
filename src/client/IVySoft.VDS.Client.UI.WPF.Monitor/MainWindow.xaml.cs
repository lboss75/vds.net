using IVySoft.VDS.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace IVySoft.VDS.Client.UI.WPF.Monitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer_;

        public MainWindow()
        {
            this.DataContext = new StatisticsDataContext();
            InitializeComponent();
        }

        new StatisticsDataContext DataContext
        {
            get
            {
                return (StatisticsDataContext)base.DataContext;
            }

            set
            {
                base.DataContext = value;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.AddServer("ws://localhost:8050/api/ws");

            this.timer_ = new System.Windows.Threading.DispatcherTimer();
            this.timer_.Tick += new EventHandler(on_timer);
            this.timer_.Interval = new TimeSpan(0, 0, 1);
            this.timer_.Start();
        }

        private async void AddServer(string service_uri)
        {
            try
            {
                using (var api = new VdsApi(new VdsApiConfig { ServiceUri = service_uri }))
                {
                    var stat = await api.GetStatistics();
                    this.DataContext.Servers.Add(new ServerStatisticRow(service_uri, stat));
                }
            }
            catch
            {

            }
        }

        private async void on_timer(object sender, EventArgs e)
        {
            var toRemove = new List<ServerStatisticRow>();
            foreach(var server in this.DataContext.Servers)
            {
                try
                {
                    using (var api = new VdsApi(new VdsApiConfig { ServiceUri = server.ServiceUri }))
                    {
                        var stat = await api.GetStatistics();
                        server.Update(stat);
                    }
                }
                catch
                {
                    toRemove.Add(server);
                }
            }

            foreach(var server in toRemove)
            {
                this.DataContext.Servers.Remove(server);
            }
        }

        private void Servers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(this.Servers.SelectedItems.Count == 1)
            {
                this.CurrentServer.DataContext = this.Servers.SelectedItems[0];
            }
            else
            {
                this.CurrentServer.DataContext = null;
            }

        }
    }
}

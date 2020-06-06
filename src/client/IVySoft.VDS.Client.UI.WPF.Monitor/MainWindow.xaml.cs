using IVySoft.VDS.Client.Api;
using IVySoft.VDS.Client.UI.WPF.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
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

        private Dictionary<string, Task> update_tasks_ = new Dictionary<string, Task>();

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.AddServer("ws://localhost:8050/api/ws");

            this.timer_ = new System.Windows.Threading.DispatcherTimer();
            this.timer_.Tick += new EventHandler(on_timer);
            this.timer_.Interval = new TimeSpan(0, 0, 1);
            this.timer_.Start();
        }

        private void AddServer(string service_uri)
        {
            if (!this.update_tasks_.ContainsKey(service_uri))
            {
                var servers = this.DataContext.Servers;
                ThreadPool.QueueUserWorkItem<object>((state) => { this.update_tasks_.Add(service_uri, this.add_server(servers, service_uri)); }, null, false);
            }
        }

        private async Task add_server(ObservableCollection<ServerStatisticRow> servers, string service_uri)
        {
            var new_servers = new SortedSet<string>();
            try
            {
                using (var cancel = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
                {
                    using (var api = new VdsApi(new VdsApiConfig { ServiceUri = service_uri }))
                    {
                        var stat = await api.GetStatistics(cancel.Token);
                        if (null != stat && null != stat.session && null != stat.session.items)
                        {
                            foreach (var session in stat.session.items)
                            {
                                var address = session.address;
                                if (address.StartsWith("udp6://::ffff:"))
                                {
                                    var body = address.Substring("udp6://::ffff:".Length);
                                    var pos = body.LastIndexOf(':');
                                    address = "ws://" + body.Substring(0, pos) + ":8050/api/ws";
                                }
                                else if (address.StartsWith("udp6://"))
                                {
                                    var body = address.Substring("udp6://".Length);
                                    var pos = body.LastIndexOf(':');

                                    address = "ws://[" + body.Substring(0, pos) + "]:8050/api/ws";
                                }
                                else
                                {
                                    throw new Exception("Error");
                                }
                                if (!new_servers.Contains(address))
                                {
                                    new_servers.Add(address);
                                }
                            }
                        }
                        Dispatcher.Invoke(() =>
                        {
                            servers.Add(new ServerStatisticRow(service_uri, stat));
                        });
                    }
                }
            }
            catch
            {

            }

            foreach(var address in new_servers)
            {
                Dispatcher.Invoke(() =>
                {
                    this.AddServer(address);
                });
            }
        }

        private void on_timer(object sender, EventArgs e)
        {
            var servers = this.DataContext.Servers;
            foreach(var item in this.update_tasks_)
            {
                if (item.Value.IsCompleted)
                {
                    ThreadPool.QueueUserWorkItem<object>((state) => { this.update_tasks_[item.Key] = this.update(servers, item.Key); }, null, false);
                }
            }
        }

        private async Task update(ObservableCollection<ServerStatisticRow> servers, string server_uri)
        {
            ServerStatisticRow server = servers.SingleOrDefault(x => x.ServiceUri == server_uri);
            if (null == server)
            {
                await this.add_server(servers, server_uri);
            }
            else
            {
                try
                {
                    using (var cancel = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
                    {

                        using (var api = new VdsApi(new VdsApiConfig { ServiceUri = server.ServiceUri }))
                        {
                            var stat = await api.GetStatistics(cancel.Token);
                            Dispatcher.Invoke(() => server.Update(stat));
                        }
                    }
                }
                catch
                {
                    Dispatcher.Invoke(() => servers.Remove(server));
                }
            }
        }
    }
}

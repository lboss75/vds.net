using IVySoft.VDS.Client.Api;
using IVySoft.VDS.Client.UI.Logic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IVySoft.VDS.Client.UI.WPF.Common
{
    /// <summary>
    /// Interaction logic for DistributionMapWindow.xaml
    /// </summary>
    public partial class DistributionMapWindow : Window
    {
        public DistributionMapWindow()
        {
            InitializeComponent();
        }

        public new ChannelMessageFileInfo DataContext
        {
            get
            {
                return (ChannelMessageFileInfo)base.DataContext;
            }
            set
            {
                base.DataContext = value;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Refresh();
        }

        private void Refresh()
        {
            var f = this.DataContext;

            ProgressWindow.Run(
                "Get distribution map",
                this, async token =>
                {
                    using (var s = new VdsService())
                    {
                        var map = await s.Api.GetDistributionMap(token, f.Blocks.Select(x => x.Id).ToArray());

                        Dispatcher.Invoke(() => this.update_map(map));
                    }
                });
        }

        private void update_map(SyncStatistic[] statistics)
        {
            BlocksView.Columns.Clear();

            DataTable dt = new DataTable();
            dt.Columns.Add("Block");
            BlocksView.Columns.Add(new GridViewColumn { Header = "Block", DisplayMemberBinding = new Binding("Block") });

            var columns = statistics.SelectMany(s => s.replicas.SelectMany(r => r.nodes)).Distinct().ToList();
            int index = 0;
            foreach (var column in columns)
            {
                var name = $"col{index}";
                dt.Columns.Add(name);
                BlocksView.Columns.Add(new GridViewColumn { Header = column, DisplayMemberBinding = new Binding(name) });
                ++index;
            }

            var nodes = new Dictionary<string, List<int>>();
            foreach(var stat in statistics)
            {
                index = 0;
                foreach(var r in stat.replicas)
                {
                    foreach(var n in r.nodes)
                    {
                        List<int> items;
                        if (nodes.TryGetValue(n, out items))
                        {
                            items.Add(index);
                        }
                        else
                        {
                            items = new List<int>();
                            items.Add(index);
                            nodes.Add(n, items);
                        }
                    }

                    ++index;
                }
                DataRow row = dt.NewRow();
                row[0] = stat.block;
                index = 1;
                foreach(var col in columns)
                {
                    List<int> items;
                    if (nodes.TryGetValue(col, out items))
                    {
                        row[index] = string.Join(',', items);
                    }

                    ++index;
                }
                dt.Rows.Add(row);
            }

            BlocksList.ItemsSource = dt.DefaultView;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            this.Refresh();
        }
    }
}

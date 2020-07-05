using IVySoft.VDS.Client.Api;
using IVySoft.VDS.Client.UI.Logic;
using IVySoft.VDS.Client.UI.WPF.Common;
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

namespace IVySoft.VDS.Client.UI.WPF.SmartHome
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ThisUser user_;
        public MainWindow()
        {
            this.DataContext = new MainWindowDataContext();
            InitializeComponent();
        }

        internal ThisUser User { get => this.user_; }

        internal new MainWindowDataContext DataContext
        {
            get
            {
                return (MainWindowDataContext)base.DataContext;
            }
            set
            {
                base.DataContext = value;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Login();
        }

        private void Login()
        {
            var dlg = new LoginWindow();
            dlg.Owner = this;
            if (dlg.ShowDialog() != true)
            {
                this.Close();
                return;
            }

            ProgressWindow.Run(
                "User login",
                this, async token =>
                {
                    using (var s = new VdsService())
                    {
                        try
                        {
                            this.user_ = await s.Api.Login(token, dlg.Login, dlg.Password);
                            this.OnGetChannels(await s.Api.GetChannels(token, this.user_));
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show(this, UIUtils.GetErrorMessage(ex), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                                this.Login();
                            });
                        }
                    }
                });
        }

        private void OnGetChannels(Api.Channel[] result)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.DataContext.ChannelList.Clear();
                foreach (var message in result)
                {
                    this.DataContext.ChannelList.Add(message);
                }
            });
        }

        private void DistributionMap_Click(object sender, RoutedEventArgs e)
        {
            var menu = (ContextMenu)((MenuItem)sender).Parent;

            var fb = ((ChannelImage)((StackPanel)menu.PlacementTarget).Tag).Item;
            if (null != fb)
            {
                DistributionMapWindow wnd = new DistributionMapWindow();
                wnd.DataContext = fb;
                wnd.Show();
            }
        }
    }
}

using IVySoft.VDS.Client.Api;
using IVySoft.VDS.Client.UI.Logic;
using IVySoft.VDS.Client.UI.Logic.Model;
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

namespace IVySoft.VDS.Client.UI.WPF
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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for (; ; )
            {
                var dlg = new LoginWindow();
                dlg.Owner = this;
                if (dlg.ShowDialog() != true)
                {
                    this.Close();
                    return;
                }

                using (var s = new VdsService())
                {
                    try
                    {
                        this.user_ = await s.Api.Login(dlg.Login, dlg.Password);
                        var devices = await s.Api.GetStorage(this.user_);
                        if(devices.Length == 0)
                        {
                            var folder = System.IO.Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                "vds",
                                "home",
                                this.user_.Id,
                                "storage");
                            System.IO.Directory.CreateDirectory(folder);

                            await s.Api.AllocateStorage(this.user_, folder, 4L * 1024 * 1024 * 1024);
                            this.OnGetChannels(await s.Api.GetChannels(this.user_));

                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, UIUtils.GetErrorMessage(ex), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }
                }
                break;
            }
        }


        private void OnGetChannels(ChannelMessage[] result)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.DataContext.ChannelList.Clear();
                foreach (var message in result)
                {
                    switch (message)
                    {
                        case Transactions.ChannelCreateTransaction msg:
                            {
                                this.DataContext.ChannelList.Add(msg);
                                break;
                            }
                    }
                }
            });
        }

        private async void ChannelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.DataContext.MessagesList.Clear();

            this.ChannelBody.Children.Clear();
            this.ChannelBody.Children.Add(new ucMessagerChannel
            {
                DataContext = this.DataContext
            });

            this.DataContext.SelectedChannel = (Transactions.ChannelCreateTransaction)ChannelList.SelectedItem;
            if (null != this.DataContext.SelectedChannel)
            {
                using (var s = new VdsService())
                {
                    try
                    {
                        this.WriteChannelHistory(await s.Api.GetChannelMessages(new Api.Channel(this.DataContext.SelectedChannel)));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, UIUtils.GetErrorMessage(ex), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        private void WriteChannelHistory(ChannelMessage[] result)
        {
            this.Dispatcher.Invoke(() =>
            {
                foreach (var item in result)
                {
                    switch (item)
                    {
                        case Transactions.UserMessageTransaction msg:
                            this.DataContext.MessagesList.Add(new Logic.Model.ChannelMessage(msg));
                            break;
                    }
                }
            });
        }

        private void CreateChannel_Executed(object sender, RoutedEventArgs e)
        {
            var dlg = new Channel.CreateChannel();
            dlg.Owner = this;
            if(true == dlg.ShowDialog())
            {
                this.DataContext.ChannelList.Add(dlg.CreatedChannel);
                this.ChannelList.SelectedItem = dlg.CreatedChannel;
            }
        }

        private void ViewWallet(object sender, RoutedEventArgs e)
        {
            var dlg = new WalletsWindow();
            dlg.Owner = this;
            dlg.ShowDialog();
        }
    }
}

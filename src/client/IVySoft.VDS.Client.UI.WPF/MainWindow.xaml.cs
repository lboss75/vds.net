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
        private string login;
        private string password;

        public MainWindow()
        {
            this.DataContext = new MainWindowDataContext();
            InitializeComponent();
        }

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
            VdsService.Instance.ErrorHandler += this.ApiErrorHandler;
            VdsService.Instance.OnLoginRequired += VdsService_OnLoginRequired;
            VdsService.Instance.OpenConnection();
        }

        private void VdsService_OnLoginRequired(object sender, LoginRequiredEventArg arg)
        {
            this.Dispatcher.Invoke(() =>
            {
                var dlg = new LoginWindow();
                if (dlg.ShowDialog() != true)
                {
                    this.Close();
                    return;
                }

                this.login = dlg.Login;
                this.password = dlg.Password;
                VdsService.Instance.Api.Login(dlg.Login, dlg.Password).ContinueWith(x =>
                {
                    if (x.IsFaulted)
                    {
                        this.login = string.Empty;
                        this.OnLoginError(x.Exception);
                    }
                    else
                    {
                        this.OnLoginSuccessful();
                    }
                });
            });
        }

        private void ApiErrorHandler(Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(this.login) && null != VdsService.Instance.Api)
            {
                VdsService.Instance.Api.Login(this.login, this.password).ContinueWith(x =>
                {
                    if (x.IsFaulted)
                    {
                        this.login = string.Empty;
                        this.OnLoginError(x.Exception);
                    }
                    else
                    {
                        this.OnLoginSuccessful();
                    }
                });
            }
        }

        private void OnLoginError(Exception ex)
        {
            this.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    this,
                    UIUtils.GetErrorMessage(ex),
                    "Ошибка входа",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                this.VdsService_OnLoginRequired(this, new LoginRequiredEventArg());
            });
        }
        internal void OnError(string title, Exception ex)
        {
            this.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    this,
                    UIUtils.GetErrorMessage(ex),
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            });
        }
        private void OnLoginSuccessful()
        {
            VdsService.Instance.Api.GetChannels().ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    this.OnError("Ошибка получения списка каналов", x.Exception);
                }
                else
                {
                    this.OnGetChannels(x.Result);
                    this.CheckStorage();
                }
            });
        }

        private void CheckStorage()
        {
            VdsService.Instance.Api.GetStorage().ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    this.OnError("Ошибка получения параметров хранилища", x.Exception);
                }
                else
                {
                    if(x.Result.Length == 0)
                    {
                        this.AllocateStorage();
                    }
                }
            });
        }

        private void AllocateStorage()
        {
            var folder = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "vds",
                "home",
                VdsService.Instance.Api.UserId,
                "storage");
            System.IO.Directory.CreateDirectory(folder);

            VdsService.Instance.Api.AllocateStorage(folder, 4L * 1024 * 1024 * 1024).ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    this.OnError("Ошибка создания хранилища", x.Exception);
                }
            });
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

        private void Window_Closed(object sender, EventArgs e)
        {
            VdsService.Instance.Stop();
        }

        private void ChannelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                VdsService.Instance.Api.GetChannelMessages(this.DataContext.SelectedChannel).ContinueWith(x =>
                {
                    if (x.IsFaulted)
                    {
                        this.OnError("Ошибка получения сообщений", x.Exception);
                    }
                    else
                    {
                        this.WriteChannelHistory(x.Result);
                    }
                });
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
            dlg.ShowDialog();
        }
    }
}

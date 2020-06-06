using IVySoft.VDS.Client.Api;
using IVySoft.VDS.Client.UI.Logic;
using IVySoft.VDS.Client.UI.WPF.Common;
using IVySoft.VPlatform.Utils;
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

namespace IVySoft.VDS.Client.UI.WPF.Wallet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ThisUser user_;
        private DispatcherTimer timer_;
        private bool AllocatedSpaceDragStarted_;
        private long free_size_;

        public MainWindow()
        {
            this.DataContext = new ObservableCollection<Logic.Model.WalletBalance>();
            InitializeComponent();
        }

        internal new ObservableCollection<Logic.Model.WalletBalance> DataContext
        {
            get
            {
                return (ObservableCollection<Logic.Model.WalletBalance>)base.DataContext;
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

            ProgressWindow.Run("User login", this, async token =>
            {
                using (var s = new VdsService())
                {
                    try
                    {
                        this.user_ = await s.Api.Login(token, dlg.Login, dlg.Password);
                        await this.refresh(token, s);
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() =>
                        {

                            MessageBox.Show(this, UIUtils.GetErrorMessage(ex), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                            this.Login();
                        });
                    }

                    this.timer_ = new DispatcherTimer();
                    this.timer_.Tick += new EventHandler(this.refresh);
                    this.timer_.Interval = new TimeSpan(0, 0, 30);
                    this.timer_.Start();
                }
            });
        }

        private void refresh(object sender, EventArgs e)
        {
            ProgressWindow.Run(
                "Refresh server statistics",
                this, async token =>
            {
                using (var s = new VdsService())
                {
                    await this.refresh(token, s);
                }
            });
        }

        private async Task refresh(CancellationToken token, VdsService s)
        {
            try
            {
                var items = new List<Logic.Model.WalletBalance>();

                foreach (var wallet in await s.Api.GetWallets(token, this.user_))
                {
                    foreach (var b in await s.Api.GetBalance(token, wallet))
                    {
                        items.Add(new Logic.Model.WalletBalance
                        {
                            Id = wallet.Id,
                            Name = wallet.Name,
                            Balance = b.Balance,
                            ProposedBalance = b.ProposedBalance,
                            Currency = b.Currency,
                            Issuer = b.Issuer
                        });
                    }
                }
                Dispatcher.Invoke(() =>
                {
                    Logic.CollectionUtils.Update(
                        this.DataContext,
                        items,
                        (x, y) => x.Id == y.Id,
                        (x, y) =>
                        {
                            x.Name = y.Name;
                            x.Balance = y.Balance;
                            x.ProposedBalance = y.ProposedBalance;
                            x.Currency = y.Currency;
                            x.Issuer = y.Issuer;
                        },
                        x => x);
                });

                long reserved_size = 0;
                long used_size = 0;
                long free_size = 0;

                var devices = await s.Api.GetStorage(token, this.user_);
                foreach (var d in devices)
                {
                    reserved_size += d.reserved_size;
                    used_size += d.used_size;
                    free_size += d.free_size;
                }

                if (0 == devices.Length)
                {
                    var di = new System.IO.DriveInfo(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                    free_size = di.AvailableFreeSpace;
                }
                this.free_size_ = free_size;

                var new_value = (reserved_size >= free_size) ? 100 : (reserved_size * 100 / free_size);
                if (new_value != AllocatedSpace.Value)
                {
                    AllocatedSpace.Value = new_value;
                }
                UsedSpace.Value = (used_size >= reserved_size) ? 100 : (used_size * 100 / reserved_size);

                AllocatedSpaceLabel.Content = string.Format(
                    UIResources.AllocatedSpaceLabelFormat,
                    HumanReadableFormat.GetBytesReadable(reserved_size),
                    HumanReadableFormat.GetBytesReadable(free_size));
                UsedSpaceLabel.Content = HumanReadableFormat.GetBytesReadable(used_size);
            }
            catch (Exception ex)
            {
                this.timer_.Stop();
                MessageBox.Show(this, UIUtils.GetErrorMessage(ex), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                this.timer_.Start();
            }
        }

        private async void AllocatedSpace_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ProgressWindow.Run("Allocate space", this, token => AllocateStorage(token, !this.AllocatedSpaceDragStarted_));
        }

        private void AllocatedSpace_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            this.AllocatedSpaceDragStarted_ = true;
        }

        private void AllocatedSpace_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            this.AllocatedSpaceDragStarted_ = false;
            ProgressWindow.Run("Allocate space", this, token => this.AllocateStorage(token, true));
        }

        private async Task AllocateStorage(CancellationToken token, bool complete)
        {
            try
            {
                long reserved_size;
                if (complete)
                {
                    using (var s = new VdsService())
                    {
                        var devices = await s.Api.GetStorage(token, this.user_);
                        if (devices.Length == 0)
                        {
                            var root_folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                            var di = new System.IO.DriveInfo(root_folder);
                            this.free_size_ = di.AvailableFreeSpace;
                            var folder = System.IO.Path.Combine(
                                root_folder,
                                "IVySoft",
                                "VDS",
                                "storage",
                                this.user_.Id);
                            System.IO.Directory.CreateDirectory(folder);

                            reserved_size = (long)(AllocatedSpace.Value * this.free_size_ / 100);
                            await s.Api.AllocateStorage(token, this.user_, folder, reserved_size, "share");
                        }
                        else
                        {
                            this.free_size_ = devices[0].free_size;
                            reserved_size = (long)(AllocatedSpace.Value * this.free_size_ / 100);
                            if (complete)
                            {
                                await s.Api.AllocateStorage(token, this.user_, devices[0].local_path, reserved_size, devices[0].usage_type);
                            }
                        }
                    }
                }
                else
                {
                    reserved_size = (long)(AllocatedSpace.Value * this.free_size_ / 100);
                }

                AllocatedSpaceLabel.Content = string.Format(
                    UIResources.AllocatedSpaceLabelFormat,
                    HumanReadableFormat.GetBytesReadable(reserved_size),
                    HumanReadableFormat.GetBytesReadable(this.free_size_));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, UIUtils.GetErrorMessage(ex), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}


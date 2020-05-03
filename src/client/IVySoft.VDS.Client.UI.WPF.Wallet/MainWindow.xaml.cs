using IVySoft.VDS.Client.Api;
using IVySoft.VDS.Client.UI.Logic;
using IVySoft.VDS.Client.UI.WPF.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace IVySoft.VDS.Client.UI.WPF.Wallet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ThisUser user_;

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
                        foreach (var wallet in await s.Api.GetWallets(this.user_))
                        {
                            foreach (var b in await s.Api.GetBalance(wallet))
                            {
                                this.DataContext.Add(new Logic.Model.WalletBalance
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
    }
}

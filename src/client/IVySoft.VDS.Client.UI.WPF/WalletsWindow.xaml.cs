using IVySoft.VDS.Client.UI.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IVySoft.VDS.Client.UI.WPF
{
    /// <summary>
    /// Interaction logic for WalletsWindow.xaml
    /// </summary>
    public partial class WalletsWindow : Window
    {
        public WalletsWindow()
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
            using(var s = new VdsService())
            {
                foreach(var wallet in await s.Api.GetWallets(((MainWindow)Application.Current.MainWindow).User))
                {
                    foreach(var b in await s.Api.GetBalance(wallet))
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
        }
    }
}

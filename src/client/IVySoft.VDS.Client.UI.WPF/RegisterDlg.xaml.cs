using System;
using System.Collections.Generic;
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
    /// Interaction logic for RegisterDlg.xaml
    /// </summary>
    public partial class RegisterDlg : Window
    {
        public string Login { get; set; }
        public string Password { get; set; }

        public RegisterDlg()
        {
            InitializeComponent();
        }

        private void registerBtn_Click(object sender, RoutedEventArgs e)
        {
            if(this.confirmPasswordEdit.Password != this.passwordEdit.Password)
            {
                MessageBox.Show(this, UIResources.PasswordDontMatch, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.Login = this.loginEdit.Text;
            this.Password = this.passwordEdit.Password;
            DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.loginEdit.Text = this.Login;
            this.passwordEdit.Password = this.Password;
        }
    }
}

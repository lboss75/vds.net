﻿using IVySoft.VDS.Client.UI.Logic;
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

namespace IVySoft.VDS.Client.UI.WPF.Common
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public string Login { get; set; }
        public string Password { get; set; }

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Login = loginEdit.Text;
            this.Password = passwordEdit.Password;
            this.DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loginEdit.Text = this.Login;
            passwordEdit.Password = this.Password;
        }

        private void registerBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new RegisterDlg();
            if (true == dlg.ShowDialog())
            {
                var save_cursor = Mouse.OverrideCursor;
                Mouse.OverrideCursor = Cursors.Wait;
                ProgressWindow.Run(
                    "Register user",
                    this, async token => 
                {
                    using (var s = new VdsService())
                    {
                        try
                        {
                            await s.Api.CreateUser(token, dlg.Login, dlg.Password);

                            Dispatcher.Invoke(() =>
                            {
                                Mouse.OverrideCursor = save_cursor;
                                this.Login = dlg.Login;
                                this.Password = dlg.Password;
                                this.DialogResult = true;
                            });
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                Mouse.OverrideCursor = save_cursor;

                                MessageBox.Show(
                                    this,
                                    UIUtils.GetErrorMessage(ex),
                                    this.Title,
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                            });
                        }
                    }
                });
            }
        }
    }
}

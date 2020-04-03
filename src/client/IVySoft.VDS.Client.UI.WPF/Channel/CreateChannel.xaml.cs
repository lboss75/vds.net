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

namespace IVySoft.VDS.Client.UI.WPF.Channel
{
    /// <summary>
    /// Interaction logic for CreateChannel.xaml
    /// </summary>
    public partial class CreateChannel : Window
    {
        public CreateChannel()
        {
            InitializeComponent();
        }

        public Transactions.ChannelCreateTransaction CreatedChannel { get; private set; }

        private void createBtn_Click(object sender, RoutedEventArgs e)
        {
            var original_mouse = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
            Logic.VdsService.Instance.Api.create_channel(
                Transactions.CbannelTypes.notes_channel,
                this.nameEdit.Text).ContinueWith(x =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        Mouse.OverrideCursor = original_mouse;
                        if (x.IsFaulted)
                        {
                            MessageBox.Show(this, Logic.UIUtils.GetErrorMessage(x.Exception), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            this.CreatedChannel = x.Result;
                            this.DialogResult = true;
                        }
                    });
                });
        }
    }
}

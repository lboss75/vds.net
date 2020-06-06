using IVySoft.VDS.Client.UI.WPF.Common;
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

        public Api.Channel CreatedChannel { get; private set; }

        private void createBtn_Click(object sender, RoutedEventArgs e)
        {
            var original_mouse = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;

            ProgressWindow.Run(
                "Channel create",
                this, async token =>
            {
                using (var s = new Logic.VdsService())
                {
                    try
                    {
                        this.CreatedChannel = await s.Api.create_channel(
                            token,
                            ((MainWindow)Application.Current.MainWindow).User,
                            Api.ChannelTypes.notes_channel,
                            this.nameEdit.Text);
                        Dispatcher.Invoke(() =>
                        {
                            Mouse.OverrideCursor = original_mouse;
                            this.DialogResult = true;
                        });
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Mouse.OverrideCursor = original_mouse;
                            MessageBox.Show(this, Logic.UIUtils.GetErrorMessage(ex), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                }
            });
        }
    }
}

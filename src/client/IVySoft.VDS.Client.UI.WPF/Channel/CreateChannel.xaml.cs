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

        private void createBtn_Click(object sender, RoutedEventArgs e)
        {
            Logic.VdsService.Instance.Api.Ch(this.DataContext.SelectedChannel).ContinueWith(x =>

        }
    }
}

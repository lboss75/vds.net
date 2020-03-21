using IVySoft.VDS.Client.UI.Logic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IVySoft.VDS.Client.UI.WPF
{
    /// <summary>
    /// Interaction logic for ucMessagerChannel.xaml
    /// </summary>
    public partial class ucMessagerChannel : UserControl
    {
        public ucMessagerChannel()
        {
            InitializeComponent();
        }

        private void FileHyperlink_Click(object sender, RoutedEventArgs e)
        {
            var target_folder = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");
            var fb = (Transactions.FileInfo)((Hyperlink)e.OriginalSource).Tag;

            VdsService.Instance.Dawnload(fb, target_folder).ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    ((MainWindow)Application.Current.MainWindow).OnError(
                        "Ошибка скачивания файла",
                        x.Exception);
                }
                else
                {
                    new System.Diagnostics.Process
                    {
                        StartInfo = new System.Diagnostics.ProcessStartInfo(x.Result)
                        {
                            UseShellExecute = true
                        }
                    }.Start();
                }
            });
        }

        private void AddFileBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            if (dlg.ShowDialog() != true)
            {
                return;
            }

            foreach (var f in dlg.FileNames)
            {
                FilesList.Items.Add(new System.IO.FileInfo(f));
            }
        }

        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MessageEdit.Text)
                && FilesList.Items.Count == 0)
            {
                return;
            }

            var msgItem = new Logic.Model.ChannelMessage
            {
                Message = MessageEdit.Text,
                Files = new System.Collections.ObjectModel.ObservableCollection<Logic.Model.ChannalMessageFileInfo>(),
                State = Logic.Model.MessageState.Draft
            };

            var files = new List<FileUploadStream>();
            foreach (System.IO.FileInfo item in FilesList.Items)
            {
                var f = new Logic.Model.ChannalMessageFileInfo
                {
                    Name = System.IO.Path.GetFileName(item.FullName),
                    Lenght = item.Length,
                    InProgress = true,
                    Progress = 0
                };
                msgItem.Files.Add(f);

                files.Add(new FileUploadStream
                {
                    Name = System.IO.Path.GetFileName(item.FullName),
                    SystemPath = item.FullName,
                    ProgressCallback = (x =>
                    {
                        f.Progress = x;
                        return true;
                    })
                });
            }

            ((MainWindow)Application.Current.MainWindow).DataContext.MessagesList.Add(msgItem);

            VdsService.Instance.Api.UploadFiles(
                ((MainWindow)Application.Current.MainWindow).DataContext.SelectedChannel,
                MessageEdit.Text,
                files.ToArray()).ContinueWith(x =>
                {
                    msgItem.State = Logic.Model.MessageState.Uploaded;
                    if (x.IsFaulted)
                    {
                        ((MainWindow)Application.Current.MainWindow).OnError(
                            "Отправка сообщения",
                            x.Exception);
                    }
                    else
                    {

                    }
                });
            MessageEdit.Text = string.Empty;
            FilesList.Items.Clear();

        }
    }
}

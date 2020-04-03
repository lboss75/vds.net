using IVySoft.VDS.Client.UI.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IVySoft.VDS.Client.UI.WinForms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            VdsService.Instance.ErrorHandler += this.ApiErrorHandler;
            VdsService.Instance.OnLoginRequired += VdsService_OnLoginRequired;
            VdsService.Instance.OpenConnection();
        }

        private string login;
        private string password;

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

        private delegate void LoginErrorDelegate(Exception ex);

        private void OnLoginError(Exception ex)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new LoginErrorDelegate(OnLoginError), ex);
            }
            else
            {
                MessageBox.Show(
                    this,
                    UIUtils.GetErrorMessage(ex),
                    "Ошибка входа",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                this.VdsService_OnLoginRequired(this, new LoginRequiredEventArg());
            }
        }

        private delegate void ErrorDelegate(string title, Exception ex);
        private void OnError(string title, Exception ex)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new ErrorDelegate(OnError), title, ex);
            }
            else
            {
                MessageBox.Show(
                    this,
                    UIUtils.GetErrorMessage(ex),
                    title,                    
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private delegate void LoginSuccessfulDelegate();
        private void OnLoginSuccessful()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new LoginSuccessfulDelegate(this.OnLoginSuccessful));
            }
            else
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
                    }
                });
            }
        }

        private delegate void GetChannelsDelegate(ChannelMessage[] result);

        private void OnGetChannels(ChannelMessage[] result)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new GetChannelsDelegate(this.OnGetChannels), new object[] { result });
            }
            else
            {
                foreach (var message in result)
                {
                    switch (message)
                    {
                        case Transactions.ChannelCreateTransaction msg:
                            {
                                var item = channelListView.Items.Add(msg.Name);
                                item.SubItems.Add(msg.Type);
                                item.Tag = msg;
                                break;
                            }
                    }
                }
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            VdsService.Instance.Stop();
        }

        private void channelListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.messagesList.Rtf = string.Empty;
            if(this.channelListView.SelectedItems.Count == 1)
            {
                VdsService.Instance.Api.GetChannelMessages(
                    (Transactions.ChannelCreateTransaction)this.channelListView.SelectedItems[0].Tag)
                    .ContinueWith(x =>
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

        private delegate void WriteChannelHistoryDelegate(ChannelMessage[] result);
        private void WriteChannelHistory(ChannelMessage[] result)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new WriteChannelHistoryDelegate(this.WriteChannelHistory), new object[] { result });
            }
            else
            {
                var sb = new StringBuilder();
                sb.Append("{\\rtf1\\ansi");
                foreach (var item in result)
                {
                    switch (item)
                    {
                        case Transactions.UserMessageTransaction msg:
                            sb.Append($"\\pard{msg.Message}\\line ");
                            foreach (var f in msg.Files)
                            {
                                sb.Append("{{\\field{\\*\\fldinst{HYPERLINK " + f.Id + " }}{\\fldrslt{" + f.Name + "}}}} \\line ");
                            }
                            sb.Append($"\\pard ");
                            break;
                    }
                }

                messagesList.Rtf = sb.ToString();
            }
        }

        private void addList_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            if(dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            foreach(var f in dlg.FileNames)
            {
                var item = messageFileList.Items.Add(System.IO.Path.GetFileName(f));
                item.SubItems.Add(new System.IO.FileInfo(f).Length.ToString());
                item.Tag = f;
            }
        }

        private void sendBtn_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(messageEdit.Text)
                && messageFileList.Items.Count == 0
                && this.channelListView.SelectedItems.Count != 1)
            {
                return;
            }

            var files = new List<FileUploadStream>();
            foreach (ListViewItem item in messageFileList.Items)
            {

                files.Add(new FileUploadStream
                {
                    Name = System.IO.Path.GetFileName((string)item.Tag),
                    SystemPath = (string)item.Tag
                });
            }

            VdsService.Instance.Api.UploadFiles(
                (Transactions.ChannelCreateTransaction)this.channelListView.SelectedItems[0].Tag,
                messageEdit.Text,
                files.ToArray()).ContinueWith(x =>
                {
                    if (x.IsFaulted)
                    {
                        this.OnError("Отправка сообщения", x.Exception);
                    }
                    else
                    {

                    }
                });
            messageEdit.Text = string.Empty;
            messageFileList.Items.Clear();
        }
    }
}

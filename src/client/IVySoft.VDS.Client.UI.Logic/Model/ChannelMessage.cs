using IVySoft.VDS.Client.Transactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace IVySoft.VDS.Client.UI.Logic.Model
{
    public class ChannelMessage : INotifyPropertyChanged
    {
        private string message_;
        private MessageState state_;

        public ChannelMessage()
        {
        }

        public ChannelMessage(UserMessageTransaction msg)
        {
            this.Message = msg.Message;
            this.Files = new ObservableCollection<ChannelMessageFileInfo>(
                msg.Files.Select(x => new ChannelMessageFileInfo(x)));
        }

        public string Message
        {
            get => message_; set
            {
                if (this.message_ != value)
                {
                    this.message_ = value;
                    if (null != this.PropertyChanged)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(Message)));
                    }
                }
            }
        }
        public ObservableCollection<ChannelMessageFileInfo> Files { get; set; }
        public MessageState State
        {
            get => state_; set
            {
                if (this.state_ != value)
                {
                    this.state_ = value;
                    if(null != this.PropertyChanged)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(State)));
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

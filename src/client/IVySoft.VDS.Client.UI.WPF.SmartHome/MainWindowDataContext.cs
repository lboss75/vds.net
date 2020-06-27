using IVySoft.VDS.Client.UI.Logic;
using IVySoft.VDS.Client.UI.WPF.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace IVySoft.VDS.Client.UI.WPF.SmartHome
{
    internal class MainWindowDataContext : INotifyPropertyChanged
    {
        private Api.Channel selected_channel_;

        public ObservableCollection<Api.Channel> ChannelList { get; } = new ObservableCollection<Api.Channel>();

        public Api.Channel SelectedChannel
        {
            get
            {
                return this.selected_channel_;
            }

            set
            {
                if(this.selected_channel_ != value)
                {
                    this.selected_channel_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedChannel)));
                    this.Images.Clear();

                    if (null != value)
                    {
                        ProgressWindow.Run(
                            "Get channel's messages",
                            Application.Current.MainWindow,
                            async token =>
                            {
                                using (var s = new VdsService())
                                {
                                    var files = new List<ChannelImage>();
                                    foreach (var item in await s.Api.GetChannelMessages(token, this.selected_channel_))
                                    {
                                        foreach (var file in item.Files)
                                        {
                                            files.Add(new ChannelImage(file));
                                        }
                                    }

                                    files.Sort();

                                    for(var i = 0; i < files.Count; ++i)
                                    {
                                        files[files.Count - i - 1].StartDownload();
                                    }

                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        foreach (var f in files)
                                        {
                                            this.Images.Add(f);
                                        }
                                    });
                                }

                            });
                    }
                }
            }
        }

        public ObservableCollection<ChannelImage> Images { get; } = new ObservableCollection<ChannelImage>();
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
using IVySoft.VDS.Client.Api;
using IVySoft.VDS.Client.UI.Logic;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;

namespace IVySoft.VDS.Client.UI.WPF.SmartHome
{
    public class ChannelImage : INotifyPropertyChanged, IComparable<ChannelImage>
    {
        private string image_url_;
        private string image_label_;
        private ChannelMessageFileInfo item_;
        private static Regex name_parser_ = new Regex(@"(\d+)-(\d\d\d\d)(\d\d)(\d\d)(\d\d)(\d\d)(\d\d)(\.*)", RegexOptions.Compiled);

        public ChannelImage(ChannelMessageFileInfo item)
        {
            this.item_ = item;
            this.image_label_ = item.Name;
            var m = name_parser_.Match(item.Name);
            if (m.Success)
            {
                this.TimePoint = new DateTime(int.Parse(m.Groups[2].Value), int.Parse(m.Groups[3].Value), int.Parse(m.Groups[4].Value),
                    int.Parse(m.Groups[5].Value), int.Parse(m.Groups[6].Value), int.Parse(m.Groups[7].Value), DateTimeKind.Utc);
            }
        }

        public DateTime TimePoint { get; set; }

        public ChannelMessageFileInfo Item { get => this.item_; }

        public void StartDownload()
        {
            string cached;
            if (VdsService.DownloadCache.TryGetFile(this.item_.Id, out cached))
            {
                this.ImageUrl = cached;
            }
            else
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.download_file));
            }
        }

        private void download_file(object state)
        {
            try
            {
                using (var source = new CancellationTokenSource(TimeSpan.FromSeconds(600)))
                {
                    using (var s = new VdsService())
                    {
                        var f = s.Download(source.Token, this.item_).Result;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            this.ImageUrl = f;
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                var message = UIUtils.GetErrorMessage(ex);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.ImageLabel += "(" + message + ")";
                });
            }
        }

        public int CompareTo([AllowNull] ChannelImage other)
        {
            return this.TimePoint.CompareTo(other.TimePoint);
        }

        public string ImageUrl
        {
            get => image_url_;
            set
            {
                if (this.image_url_ != value)
                {
                    this.image_url_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageUrl)));
                }
            }
        }
        public string ImageLabel
        {
            get => image_label_;
            set
            {
                if (this.image_label_ != value)
                {
                    this.image_label_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageLabel)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
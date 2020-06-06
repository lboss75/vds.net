using System;
using System.ComponentModel;
using System.IO;

namespace IVySoft.VDS.Client.UI.Logic.Files
{
    internal class LocalSystemFileListItem : IFileListItem, INotifyPropertyChanged
    {
        private bool isFolder_;
        private bool isFile_;
        private string path_;
        private string name_;
        private long size_;
        private byte[] icon_;

        public event PropertyChangedEventHandler PropertyChanged;

        public LocalSystemFileListItem(bool isFolder, string path)
        {
            this.isFolder_ = isFolder;
            this.isFile_ = !isFolder;
            this.path_ = path;
            this.name_ = Path.GetFileName(path);
            if (!isFolder)
            {
                this.size_ = new FileInfo(path).Length;
            }
            try
            {
                this.icon_ = IVySoft.VPlatform.Resources.FileIcons.Icons.FileExt2SVG(isFolder ? "folder" : Path.GetExtension(path).ToLower());
            }
            catch (Exception ex)
            {
                this.icon_ = IVySoft.VPlatform.Resources.FileIcons.Icons.FileExt2SVG("blank");
            }
        }

        internal void Update(LocalSystemFileListItem other)
        {
            this.IsFolder = other.isFolder_;
            this.IsFile = other.isFile_;
            this.FullName = other.path_;
            this.Name = other.name_;
            this.Size = other.size_;
            this.Icon = other.icon_;
        }

        public bool IsFolder
        {
            get => this.isFolder_; private set
            {
                if(this.isFolder_ != value)
                {
                    this.isFolder_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFolder)));
                }
            }
        }

        public bool IsFile { get => this.isFile_; private set
            {
                if (this.isFile_ != value)
                {
                    this.isFile_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFile)));
                }
            }
        }

        public string Name { get => this.name_; private set
            {
                if (this.name_ != value)
                {
                    this.name_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                }
            }
        }

        public long Size {get=> this.size_; private set
            {
                if (this.size_ != value)
                {
                    this.size_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Size)));
                }
            }
        }

        public byte[] Icon { get => this.icon_; private set
            {
                if (this.icon_ != value)
                {
                    this.icon_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Icon)));
                }
            }
        }

        public string FullName {get=> this.path_; private set
            {
                if (this.path_ != value)
                {
                    this.path_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FullName)));
                }
            }
        }
    }
}
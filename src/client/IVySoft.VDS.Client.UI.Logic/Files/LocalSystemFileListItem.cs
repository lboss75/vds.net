using System;
using System.IO;

namespace IVySoft.VDS.Client.UI.Logic.Files
{
    internal class LocalSystemFileListItem : IFileListItem
    {
        private readonly bool isFolder_;
        private readonly bool isFile_;
        private readonly string path_;
        private readonly string name_;
        private readonly long size_;
        private readonly byte[] icon_;

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

        public bool IsFolder => this.isFolder_;

        public bool IsFile => this.isFile_;

        public string Name => this.name_;

        public long Size => this.size_;

        public byte[] Icon => this.icon_;

        public string FullName => this.path_;
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace IVySoft.VDS.Client.UI.Logic.Files
{
    public class LocalSystemFileListSource : IFileListSource
    {
        public static readonly string LocalSystem = "Local System";
        private string root_;

        public string Kind => LocalSystem;

        public LocalSystemFileListSource(string root)
        {
            this.root_ = root;
        }

        public IFileListProvider CreateProvider()
        {
            return new LocalSystemFileListProvider(this.root_);
        }

        public override string ToString()
        {
            return this.root_;
        }
    }
}

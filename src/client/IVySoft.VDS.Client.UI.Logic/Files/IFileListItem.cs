using System;

namespace IVySoft.VDS.Client.UI.Logic.Files
{
    public interface IFileListItem
    {
        byte[] Icon { get; }
        bool IsFolder { get; }
        string Name { get; }
        long Size { get; }
        string FullName { get; }
    }
}
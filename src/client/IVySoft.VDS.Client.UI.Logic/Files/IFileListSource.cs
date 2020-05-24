namespace IVySoft.VDS.Client.UI.Logic.Files
{
    public interface IFileListSource
    {
        string Kind { get; }

        IFileListProvider CreateProvider();
    }
}
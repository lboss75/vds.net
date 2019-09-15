using CommandLine;

namespace IVySoft.VDS.Client.Cmd
{
    [Verb("upload", HelpText = "Upload file to the channel.")]
    internal class UploadOptions : BaseOptions
    {
        [Option('c', "channel", Required = true, HelpText = "Channel id")]
        public string ChannelId { get; set; }

        [Option('f', "file", Required = true, HelpText = "File path to upload")]
        public string[] FilePath { get; set; }

        [Option('n', "name", Required = false, HelpText = "File name in the channel")]
        public string[] FileName { get; set; }

        [Option('m', "message", Required = false, HelpText = "Message in the channel")]
        public string Message { get; set; }
    }
}
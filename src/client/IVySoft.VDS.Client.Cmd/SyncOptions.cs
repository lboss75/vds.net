using CommandLine;

namespace IVySoft.VDS.Client.Cmd
{
    [Verb("sync", HelpText = "Sync files to the channel.")]
    public class SyncOptions : BaseOptions
    {
        [Option('i', "id", Required = true, HelpText = "Channel id")]
        public string ChannelId { get; set; }

        [Option('d', "directory", Required = true, HelpText = "Path to directory")]
        public string DestinationPath { get; set; }

        [Option('m', "method", Required = false, HelpText = "Sync style", Default = SyncMethod.Both)]
        public SyncMethod Method { get; set; }

        [Option('c', "comment", Required = false, HelpText = "Comment message in the channel")]
        public string Comment { get; set; }
    }
}
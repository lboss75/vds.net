using CommandLine;

namespace IVySoft.VDS.Client.Cmd
{
    [Verb("channel", HelpText = "Create channel.")]
    public class CreateChannelOptions : BaseOptions
    {
        [Option('t', "channel-type", Required = false, HelpText = "Channel type")]
        public string ChannelType { get; set; } = "core.notes";

        [Option('n', "channel-name", Required = true, HelpText = "Channel name")]
        public string ChannelName { get; set; }
    }
}
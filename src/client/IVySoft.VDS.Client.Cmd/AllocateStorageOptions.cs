using CommandLine;

namespace IVySoft.VDS.Client.Cmd
{
    [Verb("storage", HelpText = "Allocate storage.")]
    public class AllocateStorageOptions : BaseOptions
    {
        [Option('r', "reserved", Required = true, HelpText = "Storage reserved size")]
        public string Length { get; set; }

        [Option('d', "directory", Required = true, HelpText = "Path to directory")]
        public string DestinationPath { get; set; }

        [Option("usage-type", Required = false, HelpText = "Storage usage type")]
        public string UsageType { get; set; }
    }
}
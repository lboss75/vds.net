using CommandLine;

namespace IVySoft.VDS.Client.Cmd
{
    public class BaseOptions
    {
        [Option('l', "login", Required = true, HelpText = "User login")]
        public string Login { get; set; }

        [Option('p', "password", Required = true, HelpText = "User password")]
        public string Password { get; set; }

        [Option('s', "server", Required = false, Default = "localhost:8050", HelpText = "server url")]
        public string Server { get; set; }

        [Option("timeout", Required = false, Default = 60, HelpText = "Timeout for operations")]
        public int Timeout { get; set; }
    }
}
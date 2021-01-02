using CommandLine;

namespace Chatterbox.Server
{

    public class ProgramOptions
    {

        [Option('p', "port")] public int Port { get; private set; } = 8000;

    }

}
using CommandLine;

namespace Chatterbox.Server;

public class ProgramOptions
{

    [Option('p', "port", Default = 8000)] public int Port { get; init; }

}
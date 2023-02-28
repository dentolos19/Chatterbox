using CommandLine;

namespace Chatterbox.Server;

public class ProgramOptions
{

    [Option('n', "name", Default = "Chatterbox Server")] public string Name { get; init; }
    [Option('p', "port", Default = 8000)] public int Port { get; init; }

}
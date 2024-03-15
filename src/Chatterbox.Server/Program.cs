using System.Net;
using System.Net.Sockets;
using Chatterbox.Core;
using CommandLine;

namespace Chatterbox.Server;

public static class Program
{
    private static string ServerName { get; set; }
    private static int ServerPort { get; set; }
    private static Logger Logger { get; set; }
    private static List<TcpConnection> Peers { get; } = [];

    private static void Main(string[] args)
    {
        // Initializes logger
        Logger = new Logger(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "logs",
            $"{DateTime.Now:yyyyMMdd_HHmmss}.log"
        ));

        // Parses given arguments
        Parser.Default
            .ParseArguments<ProgramOptions>(args)
            .WithParsed(options =>
            {
                // Makes sure the port is within the valid range
                if (options.Port is < 1024 or > 49151)
                {
                    Logger.Log("Port cannot be lower than 1024 or greater than 49151.");
                    return;
                }
                ServerName = options.Name;
                ServerPort = options.Port;
            });

        // Handles unhandled exceptions
        AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
        {
            var status = eventArgs.IsTerminating ? LoggerStatus.Error : LoggerStatus.Warning;
            Logger.Log(
                "An unhandled exception had occurred! " + ((Exception)eventArgs.ExceptionObject).Message,
                status
            );
        };

        // Handles process exit
        AppDomain.CurrentDomain.ProcessExit += delegate
        {
            foreach (var peer in Peers)
                peer.Disconnect(true);
            Logger.Dispose();
        };

        MainAsync().GetAwaiter().GetResult();
    }

    private static async Task MainAsync()
    {
        var tcpListener = new TcpListener(IPAddress.Any, ServerPort);
        tcpListener.Start(); // Starts listening for new connecting clients
        Logger.Log($"Started listening at port {ServerPort}.");
        tcpListener.BeginAcceptTcpClient(HandleNewClient, tcpListener); // Accepts any new connecting clients
        await Task.Delay(-1);
    }

    private static async void HandleNewClient(IAsyncResult result)
    {
        if (result.AsyncState is not TcpListener tcpListener)
            return;
        var tcpClient = tcpListener.EndAcceptTcpClient(result); // Receives the new connecting client
        var clientEndpoint = tcpClient.Client.RemoteEndPoint;
        Logger.Log($"A client ({clientEndpoint}) connected.");
        tcpListener.BeginAcceptTcpClient(HandleNewClient, tcpListener); // Restarts to accept any new connecting clients
        var tcpConnection = new TcpConnection(tcpClient);

        // Handles receiving messages
        tcpConnection.OnMessageReceived += async (_, args) =>
        {
            Logger.Log($"{args.Message.Username} ({clientEndpoint}): {args.Message.Message}");
            foreach (var peer in Peers) // Sends messages from the client to all peers
                await peer.SendAsync(args.Message);
        };

        // Handles disconnecting clients
        tcpConnection.OnConnectionLost += async (_, args) =>
        {
            Peers.Remove(tcpConnection); // Removes the client from the connected peer list
            tcpConnection.Disconnect(); // Disposes the client's connection
            foreach (var peer in Peers) // Notifies all connected peers of the disconnecting client
                await peer.SendAsync(new ChatMessage
                {
                    Username = ServerName,
                    Message = $"A user has disconnected from the server. Reason: {args.Reason}",
                    Sender = ChatSender.Server
                });
            Logger.Log($"A client ({clientEndpoint}) disconnected. Reason: {args.Reason}");
        };

        // Notifies all connected peers of the new client
        foreach (var peer in Peers)
            await peer.SendAsync(new ChatMessage
            {
                Username = ServerName,
                Message = "A user has connected to the server.",
                Sender = ChatSender.Server
            });

        // Adds the new client to the connected peer list
        Peers.Add(tcpConnection);
    }
}
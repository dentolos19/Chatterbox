using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Chatterbox.Core;
using CommandLine;

namespace Chatterbox.Server;

public static class Program
{

    private static readonly string LogsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

    private static string ServerName { get; set; }
    private static int ServerPort { get; set; }
    private static Logger Logger { get; set; }

    private static List<TcpConnection> ConnectedPeers { get; } = new();

    private static void Main(string[] args)
    {
        if (!Directory.Exists(LogsPath)) // creates a new directory for logs; if it doesn't exist
            Directory.CreateDirectory(LogsPath);
        Logger = new Logger(Path.Combine(LogsPath, $"{DateTime.Now:yyyyMMdd_HHmmss}.log")); // starts
        Parser.Default.ParseArguments<ProgramOptions>(args).WithParsed(options => // parses arguments
        {
            if (options.Port is < 1024 or > 49151)
            {
                Logger.Log("Port cannot be lower than 1024 or greater than 49151.");
                return;
            }
            ServerName = options.Name;
            ServerPort = options.Port;
        });
        AppDomain.CurrentDomain.ProcessExit += delegate { Logger.Dispose(); };
        MainAsync().GetAwaiter().GetResult();
    }

    private static async Task MainAsync()
    {
        var tcpListener = new TcpListener(IPAddress.Any, ServerPort);
        tcpListener.Start(); // starts listening for new connecting clients
        Logger.Log($"Started hosting at port {ServerPort}.");
        tcpListener.BeginAcceptTcpClient(HandleNewClient, tcpListener); // accepts any new connecting clients
        await Task.Delay(-1);
    }

    private static async void HandleNewClient(IAsyncResult result)
    {
        if (result.AsyncState is not TcpListener tcpListener)
            return;
        var tcpClient = tcpListener.EndAcceptTcpClient(result); // receives the new connecting client
        var clientEndpoint = tcpClient.Client.RemoteEndPoint;
        Logger.Log($"A client ({clientEndpoint}) connected.");
        tcpListener.BeginAcceptTcpClient(HandleNewClient, tcpListener); // restarts to accept any new connecting clients
        var tcpConnection = new TcpConnection(tcpClient);
        tcpConnection.OnMessageReceived += async (_, args) => // handle receiving messages from the client
        {
            Logger.Log($"{args.Message.Username} ({clientEndpoint}): {args.Message.Message}");
            foreach (var peer in ConnectedPeers) // sends messages from the client to all peers
                await peer.SendAsync(args.Message);
        };
        tcpConnection.OnConnectionLost += async (_, args) => // handle the client when disconnecting
        {
            ConnectedPeers.Remove(tcpConnection); // removes the client from the connected peer list
            tcpConnection.Dispose(); // disposes the client's connection
            foreach (var peer in ConnectedPeers) // notifies all connected peers of the disconnecting client
                await peer.SendAsync(new ChatMessage
                {
                    Username = ServerName,
                    Message = $"A user has disconnected from the server. Reason: {args.Reason}",
                    Sender = ChatSender.Server
                });
            Logger.Log($"A client ({clientEndpoint}) disconnected. Reason: {args.Reason}");
        };
        foreach (var peer in ConnectedPeers)
            await peer.SendAsync(new ChatMessage // notifies all connected peers of the new client
            {
                Username = ServerName,
                Message = "A user has connected to the server.",
                Sender = ChatSender.Server
            });
        ConnectedPeers.Add(tcpConnection); // adds the new client to the connected peer list
    }

}
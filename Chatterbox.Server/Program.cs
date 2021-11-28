using Chatterbox.Core;
using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Chatterbox.Server;

public static class Program
{

    private static string ServerName { get; set; }
    private static int ServerPort { get; set; }
    private static Logger Logger { get; set; }

    private static List<TcpConnection> Peers { get; } = new();

    private static void Main(string[] args)
    {
        Logger = new Logger(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", $"{DateTime.Now:yyyyMMdd_HHmmss}.log"));
        Parser.Default.ParseArguments<ProgramOptions>(args).WithParsed(options => // parses arguments
        {
            if (options.Port is < 1024 or > 49151) // checks port argument
            {
                Logger.Log("Port cannot be lower than 1024 or greater than 49151.");
                return;
            }
            ServerName = options.Name;
            ServerPort = options.Port;
        });
        AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
        {
            var status = eventArgs.IsTerminating ? LoggerStatus.Error : LoggerStatus.Warning;
            Logger.Log("An unhandled exception had occurred! " + ((Exception)eventArgs.ExceptionObject).Message, status);
        };
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
        tcpListener.Start(); // starts listening for new connecting clients
        Logger.Log($"Started listening at port {ServerPort}.");
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
            foreach (var peer in Peers) // sends messages from the client to all peers
                await peer.SendAsync(args.Message);
        };
        tcpConnection.OnConnectionLost += async (_, args) => // handle the client when disconnecting
        {
            Peers.Remove(tcpConnection); // removes the client from the connected peer list
            tcpConnection.Disconnect(); // disposes the client's connection
            foreach (var peer in Peers) // notifies all connected peers of the disconnecting client
                await peer.SendAsync(new ChatMessage
                {
                    Username = ServerName,
                    Message = $"A user has disconnected from the server. Reason: {args.Reason}",
                    Sender = ChatSender.Server
                });
            Logger.Log($"A client ({clientEndpoint}) disconnected. Reason: {args.Reason}");
        };
        foreach (var peer in Peers) // notifies all connected peers of the new client
            await peer.SendAsync(new ChatMessage
            {
                Username = ServerName,
                Message = "A user has connected to the server.",
                Sender = ChatSender.Server
            });
        Peers.Add(tcpConnection); // adds the new client to the connected peer list
    }

}
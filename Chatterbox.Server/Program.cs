using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Chatterbox.Core;
using CommandLine;

namespace Chatterbox.Server
{

    public static class Program
    {

        private static readonly string LogsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

        private static Logger Logger { get; set; }
        private static List<TcpConnection> Peers { get; set; }
        private static ushort Port { get; set; }

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += delegate { Logger.Dispose(); };
            if (!Directory.Exists(LogsPath))
                Directory.CreateDirectory(LogsPath);
            Logger = new Logger(Path.Combine(LogsPath, $"{DateTime.Now:yyyyMMdd_HHmmss}.log"));
            Peers = new List<TcpConnection>();
            Parser.Default.ParseArguments<ProgramOptions>(args).WithParsed(options =>
            {
                if (options.Port < 1024 || options.Port > 49151)
                {
                    Port = (ushort)new ProgramOptions().Port;
                    Logger.Log("Port cannot be lower than 1024 or greater than 49151.");
                }
                else
                {
                    Port = (ushort)options.Port;
                }
            });
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            var listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
            Logger.Log($"Started hosting at port {Port}.");
            listener.BeginAcceptTcpClient(HandleClient, listener);
            await Task.Delay(-1);
        }

        private static void HandleClient(IAsyncResult result)
        {
            if (result.AsyncState is not TcpListener listener)
                return;
            var client = listener.EndAcceptTcpClient(result);
            var endpoint = client.Client.RemoteEndPoint;
            Logger.Log($"A client connected from {endpoint}.");
            listener.BeginAcceptTcpClient(HandleClient, listener);
            var connection = new TcpConnection(client);
            connection.OnMessageReceived += async (_, args) =>
            {
                Logger.Log($"{args.Message.Username}: {args.Message.Content}");
                foreach (var peer in Peers)
                    await peer.SendAsync(args.Message);
            };
            connection.OnConnectionLost += (_, args) =>
            {
                connection.Dispose();
                Peers.Remove(connection);
                Logger.Log($"A client disconnected from {endpoint}. Reason: {args.Reason}");
            };
            Peers.Add(connection);
        }

    }

}
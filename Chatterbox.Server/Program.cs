using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Chatterbox.Core;
using CommandLine;

namespace Chatterbox.Server
{

    public static class Program
    {

        private static CbLogger Logger { get; } = new CbLogger();
        private static List<TcpConnection> Peers { get; } = new List<TcpConnection>();

        private static ushort Port { get; set; } = 8000;

        private static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            Parser.Default.ParseArguments<ProgramOptions>(args).WithParsed(options =>
            {
                if (options.Port == 0)
                {
                    // do nothing
                }
                else if (options.Port < 1024 || options.Port > 49151)
                {
                    Logger.Log("Port cannot be lower than 1024 or greater than 49151.");
                }
                else
                {
                    Port = (ushort)options.Port;
                }
            });
            var listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
            Logger.Log($"Started hosting at port {Port}.");
            listener.BeginAcceptTcpClient(HandleClient, listener);
            await Task.Delay(-1);
        }

        private static void HandleClient(IAsyncResult result)
        {
            if (!(result.AsyncState is TcpListener listener))
                return;
            var client = listener.EndAcceptTcpClient(result);
            var endpoint = client.Client.RemoteEndPoint;
            Logger.Log($"A client connected from {endpoint}.");
            listener.BeginAcceptTcpClient(HandleClient, listener);
            var connection = new TcpConnection(client);
            connection.OnMessageReceived += (sender, args) =>
            {
                Logger.Log($"{args.Message.Username}: {args.Message.Message}");
                foreach (var peer in Peers)
                    peer.Send(args.Message);
            };
            connection.OnConnectionLost += (sender, args) =>
            {
                connection.Dispose();
                Peers.Remove(connection);
                Logger.Log($"A client disconnected from {endpoint}.");

            };
            Peers.Add(connection);
        }

    }

}
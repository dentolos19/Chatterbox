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

        private const ushort DefaultPort = 8000;

        private static ushort Port { get; set; }

        private static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();

        private static async Task MainAsync(IEnumerable<string> args)
        {
            Port = DefaultPort;
            Parser.Default.ParseArguments<ProgramOptions>(args).WithParsed(options =>
            {
                if (options.Port == 0)
                {
                    // do nothing
                }
                else if (options.Port < 1024 || options.Port > 49151)
                {
                    Log("Port cannot be lower than 1024 or greater than 49151.");
                }
                else
                {
                    Port = (ushort)options.Port;
                }
            });
            var listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
            Log($"Started listening at port {Port}");
            listener.BeginAcceptTcpClient(HandleClient, listener);
            await Task.Delay(-1);
        }

        private static void HandleClient(IAsyncResult result)
        {
            if (!(result.AsyncState is TcpListener listener))
                return;
            var client = listener.EndAcceptTcpClient(result);
            listener.BeginAcceptTcpClient(HandleClient, listener);
            Log("A client connected.");
            var stream = client.GetStream();
            var reader = new StreamReader(stream);
            while (client.Connected)
            {
                if (client.Client.Poll(0, SelectMode.SelectRead))
                {
                    var buffer = new byte[1];
                    if (client.Client.Receive(buffer, SocketFlags.Peek) == 0)
                        break;
                }
                var data = reader.ReadLine();
                if (string.IsNullOrEmpty(data))
                    continue;
                // TODO: Send message to all other connected client
            }
            Log("A client disconnected.");
        }

        private static void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd @ HH:mm:ss}] {message}");
        }

    }

}
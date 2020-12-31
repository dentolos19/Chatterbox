using System;
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

        private static CbLogger Logger { get; } = new CbLogger();

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
            Logger.Log($"A client connected with IP of {endpoint}.");
            listener.BeginAcceptTcpClient(HandleClient, listener);
            var reader = new StreamReader(client.GetStream());
            while (client.Connected)
            {
                try
                {
                    if (client?.Client == null || !client.Client.Connected)
                        break;
                    if (!client.Client.Poll(0, SelectMode.SelectRead))
                        continue;
                    var buffer = new byte[1];
                    if (client.Client.Receive(buffer, SocketFlags.Peek) == 0)
                        break;
                }
                catch
                {
                    break;
                }
                var received = reader.ReadLine();
                if (string.IsNullOrEmpty(received))
                    continue;
                var parsed = CbMessage.Parse(received);
                Logger.Log($"{parsed.Username}: {parsed.Content}");

            }
            Logger.Log($"A client disconnected with IP of {endpoint}.");
        }

    }

}
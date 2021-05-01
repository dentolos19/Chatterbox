using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Chatterbox.Core.Events;

namespace Chatterbox.Core
{

    public class TcpConnection : IDisposable
    {

        private readonly TcpClient _client;
        private readonly StreamReader _reader;
        private readonly BackgroundWorker _receiver;
        private readonly StreamWriter _writer;

        private bool _isConnectionLost;
        private bool _isDisposed;

        public TcpConnection(TcpClient client)
        {
            _client = client;
            _reader = new StreamReader(_client.GetStream());
            _writer = new StreamWriter(_client.GetStream()) { AutoFlush = true };
            _receiver = new BackgroundWorker { WorkerSupportsCancellation = true };
            _receiver.DoWork += ReceiveData;
            _receiver.RunWorkerAsync();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;
            SendAsync(new ChatMessage { Command = ChatCommand.Disconnect }).GetAwaiter().GetResult();
            _reader.Close();
            _writer.Close();
            if (_receiver.IsBusy)
                _receiver.CancelAsync();
            _receiver.Dispose();
            if (_client.Connected)
                _client.GetStream().Close();
            _client.Close();
            _isDisposed = true;
        }

        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;
        public event EventHandler<ConnectionLostEventArgs> OnConnectionLost;

        private void ReceiveData(object? sender, DoWorkEventArgs args)
        {
            while (_client.Connected)
            {
                string? received;
                try
                {
                    received = _reader.ReadLine();
                }
                catch (Exception error)
                {
                    if (_isConnectionLost)
                        break;
                    _isConnectionLost = true;
                    OnConnectionLost?.Invoke(this, new ConnectionLostEventArgs { Reason = _isDisposed ? "Disconnected by user." : error.Message });
                    break;
                }
                if (string.IsNullOrEmpty(received))
                    continue;
                var parsed = ChatMessage.Parse(received);
                if (parsed.Command == ChatCommand.Disconnect)
                {
                    if (_isConnectionLost)
                        break;
                    _isConnectionLost = true;
                    OnConnectionLost?.Invoke(this, new ConnectionLostEventArgs { Reason = "Disconnected by user." });
                    break;
                }
                OnMessageReceived?.Invoke(this, new MessageReceivedEventArgs { Message = parsed });
            }
        }

        public async Task SendAsync(ChatMessage message)
        {
            if (_isDisposed)
                return;
            await _writer.WriteLineAsync(message.ToString());
        }

    }

}
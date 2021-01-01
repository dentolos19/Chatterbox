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
        private readonly StreamWriter _writer;
        private readonly BackgroundWorker _receiver;

        private bool _isConnectionLost;

        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;
        public event EventHandler<ConnectionLostEventArgs> OnConnectionLost;

        public bool IsConnected { get { try { if (_client?.Client == null || !_client.Client.Connected) return false; if (!_client.Client.Poll(0, SelectMode.SelectRead)) return true; var buff = new byte[1]; return _client.Client.Receive(buff, SocketFlags.Peek) != 0; } catch { return false; } } }
        public bool IsDisposed { get; private set; }

        public TcpConnection(TcpClient client)
        {
            _client = client;
            _reader = new StreamReader(_client.GetStream());
            _writer = new StreamWriter(_client.GetStream()) { AutoFlush = true };
            _receiver = new BackgroundWorker { WorkerSupportsCancellation = true };
            _receiver.DoWork += ReceiveData;
            _receiver.RunWorkerAsync();
        }

        private void ReceiveData(object sender, DoWorkEventArgs args)
        {
            while (_client.Connected)
            {
                string received;
                try
                {
                    received = _reader.ReadLine();
                }
                catch (Exception error)
                {
                    if (_isConnectionLost)
                        break;
                    _isConnectionLost = true;
                    OnConnectionLost?.Invoke(this, new ConnectionLostEventArgs { Reason = IsDisposed ? "Disconnected by user." : error.Message });
                    break;
                }
                if (string.IsNullOrEmpty(received))
                    continue;
                var parsed = CbMessage.Parse(received);
                if (parsed.RequestDisconnect)
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

        public async Task SendAsync(CbMessage message)
        {
            if (IsDisposed)
                return;
            await _writer.WriteLineAsync(message.ToString());
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;
            SendAsync(new CbMessage { RequestDisconnect = true }).GetAwaiter().GetResult();
            _reader.Dispose();
            _writer.Dispose();
            if (_receiver.IsBusy)
                _receiver.CancelAsync();
            _receiver.Dispose();
            if (_client.Connected)
                _client.GetStream().Close();
            _client.Close();
            IsDisposed = true;
        }

    }

}
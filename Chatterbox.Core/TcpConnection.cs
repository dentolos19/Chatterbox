using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using Chatterbox.Core.Events;

namespace Chatterbox.Core
{

    public class TcpConnection : IDisposable
    {
        
        private readonly TcpClient _client;
        private readonly StreamReader _reader;
        private readonly BackgroundWorker _receiver;
        private readonly StreamWriter _writer;
        private readonly BackgroundWorker _sender;

        private bool _isConnectionLost;
        private CbMessage _toBeSent;

        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;
        public event EventHandler<ConnectionLostEventArgs> OnConnectionLost;

        public bool IsConnected { get { try { if (_client?.Client == null || !_client.Client.Connected) return false; if (!_client.Client.Poll(0, SelectMode.SelectRead)) return true; var buff = new byte[1]; return _client.Client.Receive(buff, SocketFlags.Peek) != 0; } catch { return false; } } }
        public bool IsDisposed { get; private set; }

        public TcpConnection(TcpClient tcpClient)
        {
            _client = tcpClient;
            var stream = tcpClient.GetStream();
            _reader = new StreamReader(stream);
            _receiver = new BackgroundWorker { WorkerSupportsCancellation = true };
            _writer = new StreamWriter(stream) { AutoFlush = true };
            _sender = new BackgroundWorker { WorkerSupportsCancellation = true };
            _receiver.DoWork += ReceiveData;
            _sender.DoWork += SendData;
            _receiver.RunWorkerAsync();
        }

        private void ReceiveData(object sender, DoWorkEventArgs args)
        {
            while (_client.Connected)
            {
                if (!IsConnected)
                {
                    if (_isConnectionLost)
                        return;
                    _isConnectionLost = true;
                    OnConnectionLost?.Invoke(this, new ConnectionLostEventArgs());
                    return;
                }
                string received;
                try
                {
                    received = _reader.ReadLine();
                }
                catch (Exception error)
                {
                    _isConnectionLost = true;
                    OnConnectionLost?.Invoke(this, new ConnectionLostEventArgs { Reason = IsDisposed ? "Disconnected by user." : error.Message });
                    return;
                }
                if (string.IsNullOrEmpty(received))
                    return;
                OnMessageReceived?.Invoke(this, new MessageReceivedEventArgs { Message = CbMessage.Parse(received) });
            }
        }

        private void SendData(object sender, DoWorkEventArgs args)
        {
            _writer.WriteLine(_toBeSent.ToString());
            _sender.CancelAsync();
        }

        public void Send(CbMessage message)
        {
            if (IsDisposed)
                return;
            _toBeSent = message;
            _sender.RunWorkerAsync();
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;
            _reader.Dispose();
            if (_receiver.IsBusy)
                _receiver.CancelAsync();
            _receiver.Dispose();
            _writer.Dispose();
            if (_sender.IsBusy)
                _sender.CancelAsync();
            _sender.Dispose();
            if (_client.Connected)
                _client.GetStream().Close();
            _client.Close();
            IsDisposed = true;
        }

    }

}
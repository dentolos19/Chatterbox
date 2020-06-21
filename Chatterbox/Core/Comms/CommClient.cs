using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Chatterbox.Core.Comms
{

    public class CommClient
    {

        private readonly BackgroundWorker _sender;
        private readonly BackgroundWorker _receiver;

        private TcpClient _client;
        private CommMessage _toBeSent;

        private StreamReader _reader;
        private StreamWriter _writer;

        public CommMessage ReceivedMessage { get; private set; }

        public event EventHandler OnReceive;

        public CommClient()
        {
            _sender = new BackgroundWorker();
            _receiver = new BackgroundWorker();
            _sender.DoWork += SendData;
            _receiver.DoWork += ReceiveData;
            _sender.WorkerSupportsCancellation = true;
        }

        public async void Host(int port)
        {
            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            _client = await listener.AcceptTcpClientAsync();
            listener.Stop();
            _writer = new StreamWriter(_client.GetStream());
            _reader = new StreamReader(_client.GetStream());
            _writer.AutoFlush = true;
            _receiver.RunWorkerAsync();
        }

        public void Connect(IPEndPoint endpoint)
        {
            _client = new TcpClient();
            _client.Connect(endpoint);
            _writer = new StreamWriter(_client.GetStream());
            _reader = new StreamReader(_client.GetStream());
            _writer.AutoFlush = true;
            _receiver.RunWorkerAsync();
        }

        public void Send(CommMessage message)
        {
            _toBeSent = message;
            _sender.RunWorkerAsync();
        }

        public void Stop()
        {
            _client.GetStream().Close();
            _client.Close();
            _writer.Close();
            _reader.Close();
        }

        private void SendData(object sender, DoWorkEventArgs args)
        {
            _writer.WriteLine(_toBeSent.ToString());
            _sender.CancelAsync();
        }

        private void ReceiveData(object sender, DoWorkEventArgs args)
        {
            while (_client.Connected)
            {
                var data = _reader.ReadLine();
                ReceivedMessage = CommMessage.Parse(data);
                OnReceive?.Invoke(this, new EventArgs());
            }
        }

    }

}
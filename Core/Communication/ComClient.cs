﻿using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Open.Nat;

namespace Chatterbox.Core.Communication
{

    public class ComClient
    {

        private ComMessage _toBeSent;
        private StreamWriter _writer;
        private StreamReader _reader;
        private TcpClient _client;

        private readonly BackgroundWorker _sender = new BackgroundWorker();
        private readonly BackgroundWorker _reciever = new BackgroundWorker();

        public ComMessage Recieved { get; private set; }

        public event EventHandler OnRecieved;

        public ComClient()
        {
            _sender.DoWork += SendData;
            _reciever.DoWork += RecieveData;
            _sender.WorkerSupportsCancellation = true;
        }

        private void RecieveData(object sender, DoWorkEventArgs e)
        {
            while (_client.Connected)
            {
                var data = _reader.ReadLine();
                Recieved = ComMessage.Parse(data);
                OnRecieved?.Invoke(this, new EventArgs());
            }
        }

        private void SendData(object sender, DoWorkEventArgs e)
        {
            _writer.WriteLine(_toBeSent.ToString());
            _sender.CancelAsync();
        }

        public void Connect(IPEndPoint endpoint)
        {
            _client = new TcpClient();
            _client.Connect(endpoint);
            _writer = new StreamWriter(_client.GetStream());
            _reader = new StreamReader(_client.GetStream());
            _writer.AutoFlush = true;
            _reciever.RunWorkerAsync();
        }

        public async void Host(int port, bool usePortForwarding = false)
        {
            if (usePortForwarding)
            {
                var discoverer = new NatDiscoverer();
                var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, new CancellationTokenSource(10000));
                await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, port + 1, port, "Chatterbox"));
            }
            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            _client = await listener.AcceptTcpClientAsync();
            listener.Stop();
            _writer = new StreamWriter(_client.GetStream());
            _reader = new StreamReader(_client.GetStream());
            _writer.AutoFlush = true;
            _reciever.RunWorkerAsync();
        }

        public void Send(ComMessage message)
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

    }

}
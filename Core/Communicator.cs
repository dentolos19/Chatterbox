﻿using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Chatterbox.Core
{

    public class Communicator
    {

        private StreamReader _reader;
        private StreamWriter _writer;
        private TcpClient _client;
        private TcpListener _listener;
        private Relay _toBeSent;

        private bool _imHost;

        public Relay Recieved { get; private set; }

        private readonly BackgroundWorker _reciever = new BackgroundWorker();
        private readonly BackgroundWorker _sender = new BackgroundWorker();

        public event EventHandler OnRecieved;

        public Communicator()
        {
            _reciever.DoWork += RecieveData;
            _sender.DoWork += SendData;
            _sender.WorkerSupportsCancellation = true;
        }

        private void SendData(object sender, DoWorkEventArgs e)
        {
            _writer.WriteLine(_toBeSent.ToString());
            _sender.CancelAsync();
        }

        private void RecieveData(object sender, DoWorkEventArgs e)
        {
            while (_client.Connected)
            {
                try
                {
                    var data = _reader.ReadLine();
                    Recieved = Relay.Parse(data);
                    if (Recieved.IsEnding)
                    {
                        Stop();
                        return;
                    }
                    OnRecieved?.Invoke(this, new EventArgs());
                }
                catch
                {
                    Stop();
                }
            }
        }

        public void Connect(IPEndPoint endpoint)
        {
            _client = new TcpClient();
            _client.Connect(endpoint);
            _writer = new StreamWriter(_client.GetStream());
            _reader = new StreamReader(_client.GetStream());
            _writer.AutoFlush = true;
            _imHost = false;
            _reciever.RunWorkerAsync();
        }

        public void Host(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _client = _listener.AcceptTcpClient();
            _writer = new StreamWriter(_client.GetStream());
            _reader = new StreamReader(_client.GetStream());
            _writer.AutoFlush = true;
            _imHost = true;
            _reciever.RunWorkerAsync();
        }

        public void Stop()
        {
            Send(new Relay
            {
                IsEnding = true
            });
            if (_imHost)
            {
                _listener.Stop();
            }
            _writer.Close();
            _reader.Close();
            _client.GetStream().Close();
            _client.Close();
        }

        public void Send(Relay data)
        {
            _toBeSent = data;
            _sender.RunWorkerAsync();
        }

    }

}
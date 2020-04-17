using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace Chatterbox.Graphics
{

    public partial class WnMain
    {

        private TcpClient _client;
        private StreamReader _reader;
        private StreamWriter _writer;

        private string _toBeSent;

        private readonly BackgroundWorker _reciever = new BackgroundWorker();
        private readonly BackgroundWorker _sender = new BackgroundWorker();

        public WnMain()
        {
            _reciever.DoWork += RecieveMessage;
            _sender.DoWork += SendMessage;
            _sender.WorkerSupportsCancellation = true;
            InitializeComponent();
        }

        private void RecieveMessage(object sender, DoWorkEventArgs e)
        {
            while (_client.Connected)
            {
                var recieve = _reader.ReadLine();
                Dispatcher.BeginInvoke(new Action(() => { Chat.Text += $"\n{recieve}"; }));
            }
        }

        private void SendMessage(object sender, DoWorkEventArgs e)
        {
            _writer.WriteLine($"{App.Settings.Username}: {_toBeSent}");
            Dispatcher.BeginInvoke(new Action(() => { Chat.Text += $"\nYou: {_toBeSent}"; }));
            _sender.CancelAsync();
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OpenAbout(object sender, RoutedEventArgs e)
        {
            new WnAbout().Show();
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            new WnSettings().Show();
        }

        private void Host(object sender, RoutedEventArgs e)
        {
            BtnConnect.IsEnabled = false;
            BtnHost.IsEnabled = false;
            AddressBox.IsEnabled = false;
            var listener = new TcpListener(IPAddress.Any, 8000);
            listener.Start();
            _client = listener.AcceptTcpClient();
            _writer = new StreamWriter(_client.GetStream());
            _reader = new StreamReader(_client.GetStream());
            _writer.AutoFlush = true;
            _reciever.RunWorkerAsync();
            Chat.Text = "Started hosting at port 8000.";
            BtnSend.IsEnabled = true;
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            BtnConnect.IsEnabled = false;
            BtnHost.IsEnabled = false;
            AddressBox.IsEnabled = false;
            var address = AddressBox.Text.Split(":");
            _client = new TcpClient();
            var endpoint = new IPEndPoint(IPAddress.Parse(address[0]), int.Parse(address[1]));
            _client.Connect(endpoint);
            if (_client.Connected)
            {
                _writer = new StreamWriter(_client.GetStream());
                _reader = new StreamReader(_client.GetStream());
                _writer.AutoFlush = true;
                _reciever.RunWorkerAsync();
                Chat.Text = $"Connected to host server {AddressBox.Text}";
                BtnSend.IsEnabled = true;
            }
            else
            {
                BtnConnect.IsEnabled = true;
                BtnHost.IsEnabled = true;
                AddressBox.IsEnabled = true;
                BtnSend.IsEnabled = false;
                MessageBox.Show("Unable to connect to host server!", "Chatterbox");
            }
        }

        private void Send(object sender, RoutedEventArgs e)
        {
            _toBeSent = SendBox.Text;
            SendBox.Text = string.Empty;
            _sender.RunWorkerAsync();
        }

    }

}
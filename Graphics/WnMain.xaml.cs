using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using Chatterbox.Core.Models;

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
                var data = _reader.ReadLine();
                var parsed = CbData.Parse(data);
                Dispatcher.BeginInvoke(new Action(() => { WriteToChat($"{parsed.Name}: {parsed.Message}"); }));
            }
        }

        private void SendMessage(object sender, DoWorkEventArgs e)
        {
            var data = new CbData
            {
                Name = App.Settings.Username,
                Message = _toBeSent
            }.ToString();
            _writer.WriteLine(data);
            Dispatcher.BeginInvoke(new Action(() => { WriteToChat($"\nYou: {_toBeSent}"); }));
            _sender.CancelAsync();
        }

        private void WriteToChat(string message)
        {
            if (Chat.Text == string.Empty)
                Chat.Text = message;
            else
                Chat.Text += $"\n{message.Replace("\n", string.Empty)}";
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
            
            var listener = new TcpListener(IPAddress.Any, 8000);
            listener.Start();
            _client = listener.AcceptTcpClient();
            _writer = new StreamWriter(_client.GetStream());
            _reader = new StreamReader(_client.GetStream());
            _writer.AutoFlush = true;
            _reciever.RunWorkerAsync();
            WriteToChat("Started hosting at port 8000.");
            BtnConnect.IsEnabled = false;
            BtnHost.IsEnabled = false;
            AddressBox.IsEnabled = false;
            BtnSend.IsEnabled = true;
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            var address = AddressBox.Text.Split(":");
            _client = new TcpClient();
            var endpoint = new IPEndPoint(IPAddress.Parse(address[0]), int.Parse(address[1]));
            _client.Connect(endpoint);
            _writer = new StreamWriter(_client.GetStream());
            _reader = new StreamReader(_client.GetStream());
            _writer.AutoFlush = true;
            _reciever.RunWorkerAsync();
            WriteToChat($"Connected to host server {AddressBox.Text}");
            BtnConnect.IsEnabled = false;
            BtnHost.IsEnabled = false;
            AddressBox.IsEnabled = false;
            BtnSend.IsEnabled = true;
        }

        private void Send(object sender, RoutedEventArgs e)
        {
            _toBeSent = SendBox.Text;
            SendBox.Text = string.Empty;
            _sender.RunWorkerAsync();
        }

    }

}
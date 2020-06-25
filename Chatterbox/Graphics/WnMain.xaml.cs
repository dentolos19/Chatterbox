using System;
using System.Net;
using System.Windows;
using Chatterbox.Core.Comms;
using Chatterbox.Graphics.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Chatterbox.Graphics
{

    public partial class WnMain
    {

        private bool _isRunning;
        private bool _isHost;
        private CommClient _client;
        
        public WnMain()
        {
            InitializeComponent();
        }

        private void Send(object sender, RoutedEventArgs args)
        {
            _client.Send(new CommMessage { Username = App.Settings.Username, Message = SendMessageBox.Text });
            WriteToChat(new CommMessage { Username = App.Settings.Username + " (You)", Message = SendMessageBox.Text });
            SendMessageBox.Text = string.Empty;
        }

        private async void Host(object sender, RoutedEventArgs args)
        {
            if (_isRunning)
            {
                _client.Stop();
                ConnectButton.IsEnabled = true;
                HostButton.Content = "Host";
                SendButton.IsEnabled = false;
                _isRunning = false;
                _isHost = false;
                return;
            }
            var input = await this.ShowInputAsync("Input required!", "Enter any number between 1024 and 65535 to register port.", new MetroDialogSettings { CustomResourceDictionary = App.ResourceDialog, DefaultText = new Random().Next(1024, 65535).ToString() });
            if (string.IsNullOrEmpty(input))
                return;
            _client = new CommClient();
            _client.OnReceive += ReceiveMessage;
            _client.Host(int.Parse(input));
            ConnectButton.IsEnabled = false;
            HostButton.Content = "Stop";
            _isRunning = true;
            _isHost = true;
            WriteToChat(new CommMessage { Username = "Chatterbox", Message = "Host started! Waiting for users to connect..." });
        }

        private async void Connect(object sender, RoutedEventArgs args)
        {
            if (_isRunning)
            {
                _client.Stop();
                ConnectButton.Content = "Connect";
                HostButton.IsEnabled = true;
                SendButton.IsEnabled = false;
                _isRunning = false;
                return;
            }
            var input = await this.ShowInputAsync("Input required!", "Enter any an IP address and port to connect to.", new MetroDialogSettings { CustomResourceDictionary = App.ResourceDialog, DefaultText = "127.0.0.1:8000" });
            if (string.IsNullOrEmpty(input))
                return;
            var endpoint = input.Split(":");
            _client = new CommClient();
            _client.OnReceive += ReceiveMessage;
            _client.Connect(new IPEndPoint(IPAddress.Parse(endpoint[0]), int.Parse(endpoint[1])));
            SendButton.IsEnabled = true;
            ConnectButton.Content = "Disconnect";
            HostButton.IsEnabled = false;
            _isRunning = true;
            _client.Send(new CommMessage { Username = App.Settings.Username, Command = "connectedToHost" });
            WriteToChat(new CommMessage { Username = "Chatterbox", Message = "Connected to host! You may start talking. :)" });
        }

        private void ReceiveMessage(object sender, EventArgs args)
        {
            switch (_client.ReceivedMessage.Command)
            {
                case "connectedToHost":
                    if (!_isHost)
                        return;
                    Dispatcher.Invoke(() => { SendButton.IsEnabled = true; });
                    Dispatcher.Invoke(() => { WriteToChat(new CommMessage { Username = "Chatterbox", Message = $"User \"{_client.ReceivedMessage.Username}\" has connected to server! You may start talking. :)" }); });
                    return;
            }
            Dispatcher.Invoke(() => { WriteToChat(_client.ReceivedMessage); });
        }

        private void WriteToChat(CommMessage message)
        {
            MessageStack.Items.Add(new CnMessageItem(message));
        }

        private void ShowSettings(object sender, RoutedEventArgs args)
        {
            new WnSettings { Owner = this }.Show();
        }

        private void Exit(object sender, RoutedEventArgs args)
        {
            Application.Current.Shutdown();
        }

    }

}
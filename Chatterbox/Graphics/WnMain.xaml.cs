using System;
using System.Net;
using System.Windows;
using Chatterbox.Core;
using Chatterbox.Core.Comms;
using MahApps.Metro.Controls.Dialogs;

namespace Chatterbox.Graphics
{

    public partial class WnMain
    {

        private bool _isRunning;
        private CommClient _client;
        
        public WnMain()
        {
            InitializeComponent();
        }

        private void Send(object sender, RoutedEventArgs args)
        {
            var message = new CommMessage { Username = App.Settings.Username, Message = SendMessageBox.Text };
            _client.Send(message);
            PasteToChat(message);
            SendMessageBox.Text = string.Empty;
        }

        private void Host(object sender, RoutedEventArgs args)
        {
            if (_isRunning)
            {
                _client.Stop();
                ConnectButton.IsEnabled = true;
                HostButton.Content = "Host";
                SendButton.IsEnabled = false;
                _isRunning = false;
                return;
            }
            ConnectButton.IsEnabled = false;
            HostButton.Content = "Stop";
            // var dialog = new WnInput(true)
            // {
            //     Owner = this
            // };
            // if (dialog.ShowDialog() != true)
            //     return;
            _client = new CommClient();
            _client.OnReceive += ReceiveMessage;
            // var address = Utilities.GetPublicIpAddress();
            // WriteToChat($"Hosting server at public {address}:{dialog.Port} or private 127.0.0.1:{dialog.Port + 1}!");
            // _client.Host(dialog.Port, App.Settings.UsePortForwarding);
            SendButton.IsEnabled = true;
            _isRunning = true;
        }

        private void Connect(object sender, RoutedEventArgs args)
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
            ConnectButton.Content = "Disconnect";
            HostButton.IsEnabled = false;
            // var dialog = new WnInput
            // {
            //     Owner = this
            // };
            // if (dialog.ShowDialog() != true)
            //    return;
            _client = new CommClient();
            _client.OnReceive += ReceiveMessage;
            // _client.Connect(new IPEndPoint(IPAddress.Parse(dialog.Ip), dialog.Port));
            // WriteToChat($"Connected to host server {dialog.Ip}:{dialog.Port}!");
            SendButton.IsEnabled = true;
            _isRunning = true;
        }

        private void ReceiveMessage(object sender, EventArgs args)
        {
            Dispatcher.Invoke(() => { PasteToChat(_client.ReceivedMessage); });
        }

        private void PasteToChat(CommMessage message)
        {
            // TODO
        }

        private async void ShowSettings(object sender, RoutedEventArgs args)
        {
            await this.ShowMessageAsync("Internal code message", "This function is not available yet.");
        }

        private void Exit(object sender, RoutedEventArgs args)
        {
            Application.Current.Shutdown();
        }

    }

}
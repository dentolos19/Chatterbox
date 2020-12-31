using System;
using System.Net.Sockets;
using System.Windows;
using Chatterbox.Core;
using Chatterbox.Core.Events;

namespace Chatterbox.Views
{

    public partial class MainView
    {

        private TcpConnection _connection;

        public MainView()
        {
            InitializeComponent();
            UsernameInput.Text = Environment.UserName;
            MessageInput.IsEnabled = false;
            SendButton.IsEnabled = false;
        }

        private void Connect(object sender, RoutedEventArgs args)
        {
            if (_connection == null)
            {
                var client = new TcpClient();
                try
                {
                    client.Connect(IpInput.Text, int.Parse(PortInput.Text));
                }
                catch (Exception error)
                {
                    MessageStack.Text += $"Unable to connect to host. Reason: {error.Message}\n";
                    return;
                }
                MessageStack.Text += "Connected to host.\n";
                _connection = new TcpConnection(client);
                _connection.OnMessageReceived += ReceiveMessage;
                _connection.OnConnectionLost += ConnectionLost;

                UsernameInput.IsEnabled = false;
                IpInput.IsEnabled = false;
                PortInput.IsEnabled = false;
                ConnectButton.Content = "Disconnect";

                MessageInput.IsEnabled = true;
                SendButton.IsEnabled = true;
            }
            else
            {
                _connection.Dispose();
                _connection = null;

                UsernameInput.IsEnabled = true;
                IpInput.IsEnabled = true;
                PortInput.IsEnabled = true;
                ConnectButton.Content = "Connect";

                MessageInput.IsEnabled = false;
                SendButton.IsEnabled = false;
            }
        }

        private void ReceiveMessage(object sender, MessageReceivedEventArgs args)
        {
            MessageStack.Text += $"{args.Message.Username}: {args.Message.Content}\n";
        }

        private void SendMessage(object sender, RoutedEventArgs args)
        {
            _connection?.Send(new CbMessage
            {
                Username = UsernameInput.Text,
                Content = MessageInput.Text
            });
            MessageInput.Text = string.Empty;
        }

        private void ConnectionLost(object sender, ConnectionLostEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                MessageStack.Text += $"Disconnected from host. Reason: {args.Reason}\n";
                if (_connection != null)
                    Connect(null, null);
            });
        }

    }

}
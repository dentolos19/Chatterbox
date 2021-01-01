using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Windows;
using Chatterbox.Controls;
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
            UsernameInput.Focus();
            ConnectButton.IsDefault = true;
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
                    MessageStack.Items.Add(new MessageItem(new CbMessage
                    {
                        Username = "Chatterbox",
                        Message = $"Unable to connect to host. Reason: {error.Message}",
                        Creator = CbMessageCreator.Internal
                    }));
                    return;
                }
                MessageStack.Items.Add(new MessageItem(new CbMessage
                {
                    Username = "Chatterbox",
                    Message = $"Connected to {client.Client.RemoteEndPoint}.",
                    Creator = CbMessageCreator.Internal
                }));
                _connection = new TcpConnection(client);
                _connection.OnMessageReceived += ReceiveMessage;
                _connection.OnConnectionLost += ConnectionLost;

                UsernameInput.IsEnabled = false;
                IpInput.IsEnabled = false;
                PortInput.IsEnabled = false;
                ConnectButton.IsDefault = false;
                ConnectButton.IsCancel = true;
                ConnectButton.Content = "Disconnect";

                MessageInput.IsEnabled = true;
                SendButton.IsEnabled = true;
                SendButton.IsDefault = true;
            }
            else
            {
                _connection.Dispose();
                _connection = null;

                UsernameInput.IsEnabled = true;
                IpInput.IsEnabled = true;
                PortInput.IsEnabled = true;
                ConnectButton.IsDefault = true;
                ConnectButton.IsCancel = false;
                ConnectButton.Content = "Connect";

                MessageInput.IsEnabled = false;
                MessageInput.Focus();
                SendButton.IsEnabled = false;
                SendButton.IsDefault = false;
            }
        }

        private void ReceiveMessage(object sender, MessageReceivedEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                var message = args.Message;
                if (message.Username == UsernameInput.Text)
                    message.Username += " (You)";
                MessageStack.Items.Add(new MessageItem(message));
            });
        }

        private void SendMessage(object sender, RoutedEventArgs args)
        {
            _connection?.SendAsync(new CbMessage
            {
                Username = UsernameInput.Text,
                Message = MessageInput.Text,
                Creator = CbMessageCreator.User
            });
            MessageInput.Text = string.Empty;
        }

        private void ConnectionLost(object sender, ConnectionLostEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                MessageStack.Items.Add(new MessageItem(new CbMessage
                {
                    Username = "Chatterbox",
                    Message = $"Disconnected from host. Reason: {args.Reason}",
                    Creator = CbMessageCreator.Internal
                }));
                if (_connection != null)
                    Connect(null, null);
            });
        }

        private void EnsureConnectionDisposed(object sender, CancelEventArgs args)
        {
            if (_connection == null)
                return;
            if (MessageBox.Show("Are you sure that you want to close this app while connection is still active?", "Chatterbox", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _connection?.Dispose();
            }
            else
            {
                args.Cancel = true;
            }
        }

    }

}
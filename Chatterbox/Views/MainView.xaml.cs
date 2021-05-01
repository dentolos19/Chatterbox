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

        private TcpConnection? _connection;
        private Guid _userId;

        public MainView()
        {
            InitializeComponent();
            UsernameInput.Text = Environment.UserName;
            UsernameInput.Focus();
            ConnectButton.IsDefault = true;
            MessageInput.IsEnabled = false;
            SendButton.IsEnabled = false;
        }

        private async void Connect(object sender, RoutedEventArgs args)
        {
            if (_connection == null)
            {
                ConnectButton.IsEnabled = false;
                ConnectButton.Content = "Connecting";

                var client = new TcpClient();
                try
                {
                    await client.ConnectAsync(IpInput.Text, int.Parse(PortInput.Text));
                }
                catch (Exception error)
                {
                    ConnectButton.IsEnabled = true;
                    ConnectButton.Content = "Connect";

                    DisplayMessage(new ChatMessage
                    {
                        Username = "Chatterbox",
                        Content = $"Unable to connect to host. Reason: {error.Message}",
                        Sender = ChatSender.Internal
                    });
                    return;
                }
                DisplayMessage(new ChatMessage
                {
                    Username = "Chatterbox",
                    Content = $"Connected to {client.Client.RemoteEndPoint}.",
                    Sender = ChatSender.Internal
                });
                _userId = Guid.NewGuid();
                _connection = new TcpConnection(client);
                _connection.OnMessageReceived += ReceiveMessage;
                _connection.OnConnectionLost += ConnectionLost;

                UsernameInput.IsEnabled = false;
                IpInput.IsEnabled = false;
                PortInput.IsEnabled = false;
                ConnectButton.IsEnabled = true;
                ConnectButton.IsDefault = false;
                ConnectButton.IsCancel = true;
                ConnectButton.Content = "Disconnect";

                MessageInput.IsEnabled = true;
                SendButton.IsEnabled = true;
                SendButton.IsDefault = true;

                MessageInput.Focus();
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
                SendButton.IsEnabled = false;
                SendButton.IsDefault = false;
            }
        }

        private void ReceiveMessage(object? sender, MessageReceivedEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                var message = args.Message;
                if (message.Id.Equals(_userId))
                    message.Username += " (You)";
                DisplayMessage(message);
            });
        }

        private void SendMessage(object sender, RoutedEventArgs args)
        {
            _connection?.SendAsync(new ChatMessage
            {
                Id = _userId,
                Username = UsernameInput.Text,
                Content = MessageInput.Text,
                Sender = ChatSender.User
            });
            MessageInput.Text = string.Empty;
        }

        private void ConnectionLost(object? sender, ConnectionLostEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                DisplayMessage(new ChatMessage
                {
                    Username = "Chatterbox",
                    Content = $"Disconnected from host. Reason: {args.Reason}",
                    Sender = ChatSender.Internal
                });
                if (_connection != null)
                    Connect(null!, null!);
            });
        }

        private void EnsureConnectionDisposed(object sender, CancelEventArgs args)
        {
            if (_connection == null)
                return;
            if (MessageBox.Show("Are you sure that you want to close this app while connection is still active?", "Chatterbox", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                _connection?.Dispose();
            else
                args.Cancel = true;
        }

        public void DisplayMessage(ChatMessage message)
        {
            MessageStack.Items.Add(new MessageItem(message));
        }

    }

}
using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Windows;
using Chatterbox.Core;
using Chatterbox.Core.Events;
using Chatterbox.Models;

namespace Chatterbox.Views;

public partial class MainWindow
{

    private Guid _userId;
    private TcpConnection? _tcpConnection;

    public MainWindow()
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
        if (_tcpConnection == null)
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
                    Name = "Chatterbox",
                    Text = $"Unable to connect to host. Reason: {error.Message}",
                    Sender = ChatSender.Internal
                });
                return;
            }
            DisplayMessage(new ChatMessage
            {
                Name = "Chatterbox",
                Text = $"Connected to {client.Client.RemoteEndPoint}.",
                Sender = ChatSender.Internal
            });
            _userId = Guid.NewGuid();
            _tcpConnection = new TcpConnection(client);
            _tcpConnection.OnMessageReceived += ReceiveMessage;
            _tcpConnection.OnConnectionLost += ConnectionLost;

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
            _tcpConnection.Dispose();
            _tcpConnection = null;

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
                message.Name += " (You)";
            DisplayMessage(message);
        });
    }

    private void SendMessage(object sender, RoutedEventArgs args)
    {
        _tcpConnection?.SendAsync(new ChatMessage
        {
            Id = _userId,
            Name = UsernameInput.Text,
            Text = MessageInput.Text,
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
                Name = "Chatterbox",
                Text = $"Disconnected from host. Reason: {args.Reason}",
                Sender = ChatSender.Internal
            });
            if (_tcpConnection != null)
                Connect(null!, null!);
        });
    }

    private void OnClosing(object sender, CancelEventArgs args)
    {
        if (_tcpConnection == null)
            return;
        if (MessageBox.Show("Are you sure that you want to exit while connection is still active?", "Chatterbox", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            _tcpConnection?.Dispose();
        else
            args.Cancel = true;
    }

    public void DisplayMessage(ChatMessage message)
    {
        MessageStack.Items.Add(new MessageItemModel(message));
    }

}
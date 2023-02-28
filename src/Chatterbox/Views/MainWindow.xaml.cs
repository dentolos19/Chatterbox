using Chatterbox.Core;
using Chatterbox.Core.Events;
using Chatterbox.Models;
using Chatterbox.ViewModels;
using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Windows;

namespace Chatterbox.Views;

public partial class MainWindow
{

    private Guid _userId;
    private TcpConnection? _tcpConnection;

    private MainViewModel ViewModel => (MainViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();
        UsernameInput.Text = Environment.UserName;
    }

    private void DisplayMessage(ChatMessage message)
    {
        var messageItem = new MessageItemModel(message);
        MessageStack.Items.Add(messageItem);
        MessageStack.ScrollIntoView(messageItem);
    }

    private async void OnConnect(object sender, RoutedEventArgs args)
    {
        if (_tcpConnection == null)
        {
            ViewModel.EnableConnectionInput = false;
            ConnectButton.IsEnabled = false;
            ConnectButton.Content = "Connecting";

            var tcpClient = new TcpClient();
            try
            {
                await tcpClient.ConnectAsync(IpInput.Text, int.Parse(PortInput.Text)); // attempts to connect to the server
            }
            catch (Exception error)
            {
                ViewModel.EnableConnectionInput = true;
                ConnectButton.IsEnabled = true;
                ConnectButton.Content = "Connect";

                DisplayMessage(new ChatMessage // notifies the user that the connection was unsuccessful
                {
                    Username = "Chatterbox",
                    Message = $"Unable to connect to the server. Reason: {error.Message}",
                    Sender = ChatSender.Client
                });
                return;
            }

            _userId = Guid.NewGuid(); // creates a unique id for the user
            _tcpConnection = new TcpConnection(tcpClient); // setups connection between the client and the server; after successful connection
            _tcpConnection.OnMessageReceived += OnReceiveMessage;
            _tcpConnection.OnConnectionLost += OnConnectionLost;

            DisplayMessage(new ChatMessage // notifies the user that the connection was successful
            {
                Username = "Chatterbox",
                Message = $"Connected to {tcpClient.Client.RemoteEndPoint}.",
                Sender = ChatSender.Client
            });

            ViewModel.EnableConnectionInput = false;
            ViewModel.EnableMessageSending = true;
            ConnectButton.IsEnabled = true;
            ConnectButton.Content = "Disconnect";
        }
        else
        {
            _tcpConnection.Disconnect();
            _tcpConnection = null;

            ViewModel.EnableMessageSending = false;
            ViewModel.EnableConnectionInput = true;
            ConnectButton.Content = "Connect";
        }
    }

    private void OnReceiveMessage(object? sender, MessageReceivedEventArgs args)
    {
        Dispatcher.Invoke(() =>
        {
            var message = args.Message;
            if (message.Id.Equals(_userId))
                message.Username += " (You)";
            DisplayMessage(message);
        });
    }

    private void OnSendMessage(object sender, RoutedEventArgs args)
    {
        var message = MessageInput.Text;
        if (string.IsNullOrEmpty(message))
            return;
        _tcpConnection?.SendAsync(new ChatMessage
        {
            Id = _userId,
            Username = UsernameInput.Text,
            Message = message,
            Sender = ChatSender.User
        });
        MessageInput.Text = string.Empty;
    }

    private void OnConnectionLost(object? sender, ConnectionLostEventArgs args)
    {
        Dispatcher.Invoke(() =>
        {
            DisplayMessage(new ChatMessage
            {
                Username = "Chatterbox",
                Message = $"Disconnected from the server. Reason: {args.Reason}",
                Sender = ChatSender.Client
            });
            if (_tcpConnection is not null)
                OnConnect(null, null);
        });
    }

    private void OnClosing(object sender, CancelEventArgs args)
    {
        if (_tcpConnection is null)
            return;
        if (MessageBox.Show("Are you sure that you want to exit while connection is still active?", "Chatterbox", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            _tcpConnection?.Disconnect();
        else
            args.Cancel = true;
    }

}
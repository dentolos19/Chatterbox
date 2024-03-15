using System.ComponentModel;
using System.Net.Sockets;
using Chatterbox.Core.Events;

namespace Chatterbox.Core;

public class TcpConnection
{
    private readonly TcpClient _client;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;
    private readonly BackgroundWorker _receiver;

    private bool _isConnectionLost;
    private bool _isDisconnected;

    public event EventHandler<MessageReceivedEventArgs>? OnMessageReceived;
    public event EventHandler<ConnectionLostEventArgs>? OnConnectionLost;

    public TcpConnection(TcpClient client)
    {
        _client = client;
        _reader = new StreamReader(_client.GetStream());
        _writer = new StreamWriter(_client.GetStream()) { AutoFlush = true };
        _receiver = new BackgroundWorker { WorkerSupportsCancellation = true };
        _receiver.DoWork += ReceiveData;
        _receiver.RunWorkerAsync(); // Starts actively receiving for new messages
    }

    public async Task SendAsync(ChatMessage message)
    {
        if (!_isDisconnected)
            await _writer.WriteLineAsync(message.ToString());
    }

    public void Disconnect(bool isServer = false)
    {
        if (_isDisconnected)
            return;
        try
        {
            var message = isServer
                ? new ChatMessage { Command = ChatCommand.ServerClosing }
                : new ChatMessage { Command = ChatCommand.UserDisconnecting };
            SendAsync(message).GetAwaiter().GetResult();
        }
        catch
        {
            // Do nothing
        }
        _reader.Close();
        _writer.Close();
        if (_receiver.IsBusy)
            _receiver.CancelAsync();
        _receiver.Dispose();
        if (_client.Connected)
            _client.GetStream().Close();
        _client.Close();
        _isDisconnected = true;
    }

    private void ReceiveData(object? sender, DoWorkEventArgs args)
    {
        while (_client.Connected)
        {
            // Actively receives new messages while connected
            string? receivedData;
            try
            {
                receivedData = _reader.ReadLine();
            }
            catch (Exception error)
            {
                if (_isConnectionLost)
                    // Having an exception means that the connection is broken
                    break;
                _isConnectionLost = true;
                OnConnectionLost?.Invoke(this,
                    new ConnectionLostEventArgs { Reason = _isDisconnected ? "Disconnected by user." : error.Message });
                break;
            }

            if (string.IsNullOrEmpty(receivedData))
                // Continues the loop; if the message received was empty
                continue;

            // Try to parse the received data
            ChatMessage? message = null;
            try
            {
                message = ChatMessage.Parse(receivedData);
            }
            catch
            {
                // Do nothing
            }

            if (message is null)
                // Continues the loop; if the message is invalid or not secure
                continue;
            if (_isConnectionLost)
                // Breaks the loop; if the connection is lost
                break;

            // Handles user disconnecting command
            if (message.Command == ChatCommand.UserDisconnecting)
            {
                _isConnectionLost = true;
                OnConnectionLost?.Invoke(this, new ConnectionLostEventArgs { Reason = "Disconnected by user." });
                break;
            }

            // Handles server closing command
            if (message.Command == ChatCommand.ServerClosing)
            {
                _isConnectionLost = true;
                OnConnectionLost?.Invoke(this, new ConnectionLostEventArgs { Reason = "Server closed." });
                break;
            }

            OnMessageReceived?.Invoke(this, new MessageReceivedEventArgs { Message = message });
        }
    }
}
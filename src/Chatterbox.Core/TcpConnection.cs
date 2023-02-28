using Chatterbox.Core.Events;
using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

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
        _receiver.RunWorkerAsync(); // starts actively receiving for new messages
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
            // do nothing
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
        while (_client.Connected) // actively receives new messages while connected
        {
            string? receivedData;
            try
            {
                receivedData = _reader.ReadLine();
            }
            catch (Exception error) // having an exception means that the connection is broken
            {
                if (_isConnectionLost)
                    break;
                _isConnectionLost = true;
                OnConnectionLost?.Invoke(this, new ConnectionLostEventArgs { Reason = _isDisconnected ? "Disconnected by user." : error.Message });
                break;
            }
            if (string.IsNullOrEmpty(receivedData))
                continue; // continues the loop; if the message received was empty
            ChatMessage? message = null;
            try
            {
                message = ChatMessage.Parse(receivedData); // tries to parse the received data
            }
            catch
            {
                // do nothing
            }
            if (message is null)
                continue; // continues the loop; if the message is invalid or not secure
            if (_isConnectionLost)
                break;
            if (message.Command == ChatCommand.UserDisconnecting) // handles user disconnecting command
            {
                _isConnectionLost = true;
                OnConnectionLost?.Invoke(this, new ConnectionLostEventArgs { Reason = "Disconnected by user." });
                break;
            }
            if (message.Command == ChatCommand.ServerClosing) // handles server closing command
            {
                _isConnectionLost = true;
                OnConnectionLost?.Invoke(this, new ConnectionLostEventArgs { Reason = "Server closed." });
                break;
            }
            OnMessageReceived?.Invoke(this, new MessageReceivedEventArgs { Message = message });
        }
    }

}
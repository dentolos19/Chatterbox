using Chatterbox.Core.Events;
using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Chatterbox.Core;

public class TcpConnection : IDisposable
{

    private readonly TcpClient _client;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;
    private readonly BackgroundWorker _receiver;

    private bool _isConnectionLost;
    private bool _isDisposed;

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
        if (_isDisposed)
            return;
        await _writer.WriteLineAsync(message.ToString());
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;
        try
        {
            SendAsync(new ChatMessage { Command = ChatCommand.Disconnect }).GetAwaiter().GetResult();
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
        _isDisposed = true;
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
                OnConnectionLost?.Invoke(this, new ConnectionLostEventArgs { Reason = _isDisposed ? "Disconnected by user." : error.Message });
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
            if (message.Command == ChatCommand.Disconnect) // handles disconnect command
            {
                if (_isConnectionLost)
                    break;
                _isConnectionLost = true;
                OnConnectionLost?.Invoke(this, new ConnectionLostEventArgs { Reason = "Disconnected by user." });
                break;
            }
            OnMessageReceived?.Invoke(this, new MessageReceivedEventArgs { Message = message });
        }
    }

}
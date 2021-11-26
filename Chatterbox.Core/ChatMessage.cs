﻿using System;
using System.Text.Json;

namespace Chatterbox.Core;

public class ChatMessage
{

    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Text { get; set; }
    public ChatCommand Command { get; set; } = ChatCommand.None;
    public ChatSender Sender { get; set; } = ChatSender.Unknown;
    public DateTime Time { get; set; } = DateTime.Now;

    public static ChatMessage Parse(string data)
    {
        ChatEncryption.DecryptData(data, ChatEncryption.Key, out var json);
        return JsonSerializer.Deserialize<ChatMessage>(json)!;
    }

    public override string ToString()
    {
        var json = JsonSerializer.Serialize(this);
        return ChatEncryption.EncryptData(json, ChatEncryption.Key);
    }

}
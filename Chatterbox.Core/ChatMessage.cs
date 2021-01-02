using System;
using System.Text.Json;

namespace Chatterbox.Core
{

    public class ChatMessage
    {

        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public ChatCommand Command { get; set; } = ChatCommand.None;
        public ChatSender Sender { get; set; } = ChatSender.Unknown;
        public DateTime Time { get; set; } = DateTime.Now;

        public static ChatMessage Parse(string data)
        {
            return JsonSerializer.Deserialize<ChatMessage>(data);
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

    }

}
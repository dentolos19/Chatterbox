using Chatterbox.Core;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Chatterbox.Models;

public class MessageItemModel
{

    public BitmapImage Image { get; }
    public string Name { get; }
    public string Message { get; }

    public MessageItemModel(ChatMessage message)
    {
        Image = message.Sender switch
        {
            ChatSender.User => (BitmapImage)Application.Current.FindResource("UserImage"),
            ChatSender.Client => (BitmapImage)Application.Current.FindResource("IconImage"),
            ChatSender.Server => (BitmapImage)Application.Current.FindResource("ServerImage"),
            _ => (BitmapImage)Application.Current.FindResource("UnknownImage")
        };
        Name = message.Username + $" @ {message.Time:t}";
        Message = message.Message;
    }

}
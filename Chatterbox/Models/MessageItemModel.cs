using System.Windows;
using System.Windows.Media.Imaging;
using Chatterbox.Core;

namespace Chatterbox.Models;

public record MessageItemModel
{

    public BitmapImage Image { get; }
    public string Name { get; }
    public string Text { get; }

    public MessageItemModel(ChatMessage message)
    {
        Image = message.Sender switch
        {
            ChatSender.User => (BitmapImage)Application.Current.FindResource("ImgUser"),
            ChatSender.Internal => (BitmapImage)Application.Current.FindResource("ImgHost"),
            _ => (BitmapImage)Application.Current.FindResource("ImgUnknown")
        };
        Name = message.Name + $" @ {message.Time:t}";
        Text = message.Text;
    }

}
using System.Windows;
using System.Windows.Media.Imaging;
using Chatterbox.Core;

namespace Chatterbox.Controls
{

    public partial class MessageItem
    {

        private readonly ChatMessage _originalMessage;

        public MessageItem(ChatMessage message)
        {
            InitializeComponent();
            _originalMessage = message;
            ProfileIcon.Source = _originalMessage.Sender switch
            {
                ChatSender.User => (BitmapImage)FindResource("ImgUser"),
                ChatSender.Internal => (BitmapImage)FindResource("ImgHost"),
                _ => (BitmapImage)FindResource("ImgUnknown")
            };
            UsernameText.Text = _originalMessage.Username + $" @ {_originalMessage.Time:t}";
            MessageText.Text = _originalMessage.Message;
        }

        private void CopyUsername(object sender, RoutedEventArgs args)
        {
            Clipboard.SetText(_originalMessage.Username);
        }

        private void CopyMessage(object sender, RoutedEventArgs args)
        {
            Clipboard.SetText(_originalMessage.Message);
        }

    }

}
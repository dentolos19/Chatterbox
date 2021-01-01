using System.Windows.Media.Imaging;
using Chatterbox.Core;

namespace Chatterbox.Controls
{

    public partial class MessageItem
    {

        public MessageItem(CbMessage message)
        {
            InitializeComponent();
            ProfileIcon.Source = message.Creator switch
            {
                CbMessageCreator.User => (BitmapImage)FindResource("ImgUser"),
                CbMessageCreator.Internal => (BitmapImage)FindResource("ImgHost"),
                _ => (BitmapImage)FindResource("ImgUnknown")
            };
            UsernameText.Text = message.Username;
            MessageText.Text = message.Message;
            TimeText.Text = message.Time.ToString("t");
        }

    }

}
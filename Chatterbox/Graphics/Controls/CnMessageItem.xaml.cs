using Chatterbox.Core.Comms;

namespace Chatterbox.Graphics.Controls
{

    public partial class CnMessageItem
    {

        public CnMessageItem(CommMessage message)
        {
            InitializeComponent();
            UserName.Content = message.Username;
            UserMessage.Content = message.Message;
            MessageTime.Content = message.Time;
        }

    }

}
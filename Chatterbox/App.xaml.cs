using System.Windows;
using System.Windows.Threading;
using Chatterbox.Core;
using Chatterbox.Views;

namespace Chatterbox;

public partial class App
{

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
    {
        if (Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.DisplayMessage(new ChatMessage
            {
                Username = "Chatterbox",
                Message = $"An unhandled exception occurred! Reason: {args.Exception.Message}",
                Sender = ChatSender.Client
            });
        }
        else
        {
            MessageBox.Show("An unhandled exception occurred! " + args.Exception.Message, "Chatterbox");
        }
        args.Handled = true;
    }

}
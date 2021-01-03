using System.Windows;
using System.Windows.Threading;
using Chatterbox.Core;
using Chatterbox.Views;

namespace Chatterbox
{

    public partial class App
    {

        private void Initialize(object sender, StartupEventArgs args)
        {
            Current.MainWindow = new MainView();
            Current.MainWindow.Show();
        }

        private void HandleException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            if (Current.MainWindow is MainView view)
                view.DisplayMessage(new ChatMessage
                {
                    Username = "Chatterbox",
                    Content = $"An unhandled exception occurred! Reason: {args.Exception.Message}",
                    Sender = ChatSender.Internal
                });
            args.Handled = true;
        }

    }

}
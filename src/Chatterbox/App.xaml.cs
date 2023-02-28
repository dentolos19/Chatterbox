using System.Windows;
using System.Windows.Threading;

namespace Chatterbox;

public partial class App
{

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
    {
        MessageBox.Show("An unhandled exception occurred! " + args.Exception.Message, "Chatterbox");
        args.Handled = true;
    }

}
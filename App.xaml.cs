using System.Windows;
using System.Windows.Threading;
using Chatterbox.Core;
using Chatterbox.Graphics;

namespace Chatterbox
{

    public partial class App
    {

        internal static readonly Configuration Settings = Configuration.Load();

        private WnMain _windowMain;

        private void Initialize(object sender, StartupEventArgs e)
        {
            _windowMain = new WnMain();
            _windowMain.Show();
        }

        private void HandleException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var answer = MessageBox.Show($"An error has occurred! {e.Exception.Message} Do you want to restart?", "Chatterbox", MessageBoxButton.YesNo);
            if (answer != MessageBoxResult.Yes)
                return;
            _windowMain.Hide();
            _windowMain = new WnMain();
            _windowMain.Show();
            e.Handled = true;
        }

    }

}
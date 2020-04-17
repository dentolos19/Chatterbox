using System.Windows;
using System.Windows.Threading;
using Chatterbox.Core;
using Chatterbox.Graphics;

namespace Chatterbox
{

    public partial class App
    {

        internal static readonly Configuration Settings = Configuration.Load();

        private static WnMain _windowMain;

        private void Initialize(object sender, StartupEventArgs e)
        {
            _windowMain = new WnMain();
            _windowMain.Show();
        }

        private void HandleExceptions(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var result = MessageBox.Show($"An error has occurred! {e.Exception.Message} Do you want to continue using this program?", "Chatterbox", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                _windowMain.Hide();
                _windowMain = new WnMain();
                _windowMain.Show();
                e.Handled = true;
            }
        }

    }

}
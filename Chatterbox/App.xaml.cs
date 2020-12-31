using System.Windows;
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

    }

}
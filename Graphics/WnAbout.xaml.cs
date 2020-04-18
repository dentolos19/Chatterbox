using System.Windows;
using System.Windows.Media;
using Chatterbox.Core;

namespace Chatterbox.Graphics
{

    public partial class WnAbout
    {

        public WnAbout()
        {
            InitializeComponent();
            if (App.Settings.AppTheme == "Dark")
                Panel.Background = new BrushConverter().ConvertFrom("#FF444444") as Brush;
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CheckForUpdates(object sender, RoutedEventArgs e)
        {
            if (!Utilities.IsUserOnline())
                return;
            if (!Utilities.IsUpdateAvailable())
                MessageBox.Show("Update is available! Check the release page for the latest version!", "Chatterbox");
        }

    }

}
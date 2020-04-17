using System.Windows;
using System.Windows.Media;

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

    }

}
using System.Windows;

namespace Chatterbox.Graphics
{

    public partial class WnSettings
    {

        public WnSettings()
        {
            InitializeComponent();
            UsernameBox.Text = App.Settings.Username;
        }

        private void Save(object sender, RoutedEventArgs args)
        {
            App.Settings.Username = UsernameBox.Text;
            App.Settings.Save();
        }

    }

}
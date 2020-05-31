using System.Windows;

namespace Chatterbox.Graphics
{

    public partial class WnSettings
    {

        public WnSettings()
        {
            InitializeComponent();
            CfgUsername.Text = App.Settings.Username;
            CfgAutoCheckUpdates.IsChecked = App.Settings.AutoCheckUpdates;
            CfgUsePortForwarding.IsChecked = App.Settings.UsePortForwarding;
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            App.Settings.Username = CfgUsername.Text;
            App.Settings.AutoCheckUpdates = CfgAutoCheckUpdates.IsChecked == true;
            App.Settings.UsePortForwarding = CfgUsePortForwarding.IsChecked == true;
            App.Settings.Save();
            MessageBox.Show("All settings saved!", "Chatterbox");
            Close();
        }

    }

}
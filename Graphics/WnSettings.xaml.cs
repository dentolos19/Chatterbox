using System.Windows;

namespace Chatterbox.Graphics
{

    public partial class WnSettings
    {

        public WnSettings()
        {
            InitializeComponent();
            CfgUsername.Text = App.Settings.Username;
            CfgHostingPort.Text = App.Settings.HostingPort.ToString();
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            App.Settings.Username = CfgUsername.Text;
            App.Settings.HostingPort = int.Parse(CfgHostingPort.Text);
            App.Settings.Save();
            MessageBox.Show("All settings saved!", "Chatterbox");
            Close();
        }

    }

}
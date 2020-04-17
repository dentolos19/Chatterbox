using System.Windows;
using System.Windows.Media;

namespace Chatterbox.Graphics
{

    public partial class WnSettings
    {

        public WnSettings()
        {
            InitializeComponent();
            if (App.Settings.AppTheme == "Dark")
                Panel.Background = new BrushConverter().ConvertFrom("#FF444444") as Brush;
            CfgUsername.Text = App.Settings.Username;
            CfgHostingPort.Text = App.Settings.HostingPort.ToString();
            CfgAppTheme.Text = App.Settings.AppTheme;
            CfgAppAccent.Text = App.Settings.AppAccent;
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            App.Settings.Username = CfgUsername.Text;
            App.Settings.HostingPort = int.Parse(CfgHostingPort.Text);
            App.Settings.AppTheme = CfgAppTheme.Text;
            App.Settings.AppAccent = CfgAppAccent.Text;
            App.Settings.Save();
            MessageBox.Show("All settings saved! Restart this app to take effect immediately!", "Chatterbox");
            Close();
        }

    }

}
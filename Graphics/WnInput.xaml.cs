using System.Windows;

namespace Chatterbox.Graphics
{

    public partial class WnInput
    {

        public int Port { get; private set; }

        public string Ip { get; private set; }

        public WnInput(bool isHost = false)
        {
            InitializeComponent();
            if (!isHost)
                return;
            IrIp.Text = "Hosting at any IP addresses";
            IrIp.IsEnabled = false;
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            Port = int.Parse(IrPort.Text);
            Ip = IrIp.Text;
            DialogResult = true;
            Close();
        }

    }

}
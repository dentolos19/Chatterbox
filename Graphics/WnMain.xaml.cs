using System;
using System.Net;
using System.Windows;
using Chatterbox.Core;

namespace Chatterbox.Graphics
{

    public partial class WnMain
    {

        private bool _isRunning;
        private readonly Communicator _communicator = new Communicator();

        public WnMain()
        {
            _communicator.OnRecieved += Recieved;
            InitializeComponent();
        }

        private void WriteToChat(string message)
        {
            BxChat.Text += message;
        }

        private void Recieved(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => { WriteToChat($"[{_communicator.Recieved.Time.ToShortTimeString()}] {_communicator.Recieved.Name}: {_communicator.Recieved.Message}"); });
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OpenAbout(object sender, RoutedEventArgs e)
        {
            new WnAbout().ShowDialog();
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            new WnSettings().ShowDialog();
        }

        private void Host(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isRunning)
                {
                    _communicator.Stop();
                    BnConnect.IsEnabled = true;
                    BnSend.IsEnabled = false;
                    BnHost.Content = "Host";
                    _isRunning = false;
                    return;
                }
                _communicator.Host(App.Settings.HostingPort);
                WriteToChat($"Started hosting at port {App.Settings.HostingPort}");
                BnConnect.IsEnabled = false;
                BnSend.IsEnabled = true;
                BnHost.Content = "Stop";
                _isRunning = true;
            }
            catch
            {
                BnConnect.IsEnabled = true;
                BnSend.IsEnabled = false;
                BnHost.Content = "Host";
                _isRunning = false;
                MessageBox.Show("Unable to start/stop hosting!", "Chatterbox");
            }
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isRunning)
                {
                    _communicator.Stop();
                    BnHost.IsEnabled = true;
                    BnSend.IsEnabled = false;
                    BnConnect.Content = "Connect";
                    _isRunning = false;
                    return;
                }
                _communicator.Connect(new IPEndPoint(IPAddress.Loopback, 8000));
                WriteToChat("Connected to host server 127.0.0.1:8000");
                BnHost.IsEnabled = false;
                BnSend.IsEnabled = true;
                BnConnect.Content = "Disconnect";
                _isRunning = true;
            }
            catch
            {
                BnHost.IsEnabled = true;
                BnSend.IsEnabled = false;
                BnConnect.Content = "Connect";
                _isRunning = false;
                MessageBox.Show("Unable to dis/connect to host server!", "Chatterbox");
            }
        }

        private void Send(object sender, RoutedEventArgs e)
        {
            var data = new Relay
            {
                Name = App.Settings.Username,
                Message = BxSend.Text
            };
            _communicator.Send(data);
            WriteToChat($"[{data.Time.ToShortTimeString()}] {data.Name}: {data.Message}");
            BxSend.Text = string.Empty;
        }

    }

}
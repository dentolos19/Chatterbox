using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using Chatterbox.Core;
using Chatterbox.Core.Communication;

namespace Chatterbox.Graphics
{

    public partial class WnMain
    {

        private bool _isRunning;
        private ComClient _client;

        public WnMain()
        {
            InitializeComponent();
        }

        private void OpenAbout(object sender, RoutedEventArgs e)
        {
            new WnAbout
            {
                Owner = this
            }.ShowDialog();
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            new WnSettings
            {
                Owner = this
            }.ShowDialog();
        }

        private void Host(object sender, RoutedEventArgs e)
        {
            if (_isRunning)
            {
                _client.Stop();
                BnConnect.IsEnabled = true;
                BnHost.Content = "Host";
                BnSend.IsEnabled = false;
                _isRunning = false;
                return;
            }
            var dialog = new WnInput(true)
            {
                Owner = this
            };
            if (dialog.ShowDialog() != true)
                return;
            _client = new ComClient();
            _client.OnRecieved += WriteRecieved;
            _client.Host(dialog.Port);
            WriteToChat($"Started hosting at {dialog.Port}!");
            BnConnect.IsEnabled = false;
            BnHost.Content = "Stop";
            BnSend.IsEnabled = true;
            _isRunning = true;
        }

        private void WriteRecieved(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                WriteToChat($"[{_client.Recieved.Time.ToShortTimeString()}] {_client.Recieved.Name}: {_client.Recieved.Message}");
            });
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            if (_isRunning)
            {
                _client.Stop();
                BnConnect.Content = "Connect";
                BnHost.IsEnabled = true;
                BnSend.IsEnabled = false;
                _isRunning = false;
                return;
            }
            var dialog = new WnInput
            {
                Owner = this
            };
            if (dialog.ShowDialog() != true)
                return;
            _client = new ComClient();
            _client.OnRecieved += WriteRecieved;
            _client.Connect(new IPEndPoint(IPAddress.Parse(dialog.Ip), dialog.Port));
            WriteToChat($"Connected to host server {dialog.Ip}:{dialog.Port}!");
            BnConnect.Content = "Disconnect";
            BnHost.IsEnabled = false;
            BnSend.IsEnabled = true;
            _isRunning = true;
        }

        private void Send(object sender, RoutedEventArgs e)
        {
            _client.Send(new ComMessage
            {
                Name = App.Settings.Username,
                Message = BxSend.Text
            });
            WriteToChat($"[{DateTime.Now.ToShortTimeString()}] You: {BxSend.Text}");
            BxSend.Text = string.Empty;
        }

        private void WriteToChat(string text)
        {
            BxChat.Text += $"\n{text}";
        }

        private void CheckForUpdates(object sender, RoutedEventArgs e)
        {
            if (!App.Settings.AutoCheckUpdates)
                return;
            if (!Utilities.IsUserOnline())
                return;
            if (!Utilities.IsUpdateAvailable())
                return;
            var result = MessageBox.Show("Update is available! Do you want to visit the downloads page?", "Chatterbox", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
                Process.Start("https://github.com/dentolos19/Chatterbox/releases");
        }

    }

}
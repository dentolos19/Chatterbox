using System;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Media;
using Chatterbox.Core;
using Chatterbox.Core.Mechanics;
using MahApps.Metro.Controls.Dialogs;

namespace Chatterbox.Graphics
{

    public partial class WnMain
    {

        private bool _isRunning;
        private readonly Communicator _communicator = new Communicator();

        public WnMain()
        {
            _communicator.OnRecieved += Recieved;
            _communicator.OnStop += Stop;
            InitializeComponent();
            if (App.Settings.AppTheme == "Dark")
                Panel.Background = new BrushConverter().ConvertFrom("#FF444444") as Brush;
        }

        private void Stop(object sender, EventArgs e)
        {
            if (BnHost.IsEnabled)
            {
                WriteToChat("Stopped host server");
                Host(null,null);
            }
            else if (BnConnect.IsEnabled)
            {
                WriteToChat("Disconnected from host server");
                Connect(null, null);
            }
        }

        private void WriteToChat(string message)
        {
            BxChat.Text += $"\n{message}";
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

        private async void Host(object sender, RoutedEventArgs e)
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
                if (!Utilities.IsUserOnline())
                {
                    await this.ShowMessageAsync("I need something!", "An internet connection is required for this work!");
                    return;
                }
                _isRunning = true;
                _communicator.Host(App.Settings.HostingPort);
                WriteToChat($"Started hosting at port {Utilities.GetPublicIp()}:{App.Settings.HostingPort}");
                BnConnect.IsEnabled = false;
                BnSend.IsEnabled = true;
                BnHost.Content = "Stop";
            }
            catch
            {
                BnConnect.IsEnabled = true;
                BnSend.IsEnabled = false;
                BnHost.Content = "Host";
                if (!_isRunning)
                    WriteToChat("Unable to start hosting");
                _isRunning = false;
            }
        }

        private async void Connect(object sender, RoutedEventArgs e)
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
                if (!Utilities.IsUserOnline())
                {
                    await this.ShowMessageAsync("I need something!", "An internet connection is required for this work!");
                    return;
                }
                var raw = await this.ShowInputAsync("Your input is required!", "Enter the IP address to connect to.", new MetroDialogSettings
                {
                    DefaultText = "127.0.0.1:8000"
                });
                var address = raw.Split(":");
                _isRunning = true;
                _communicator.Connect(new IPEndPoint(IPAddress.Parse(address[0]), int.Parse(address[1])));
                WriteToChat($"Connected to host server {raw}");
                BnHost.IsEnabled = false;
                BnSend.IsEnabled = true;
                BnConnect.Content = "Disconnect";
                
            }
            catch
            {
                BnHost.IsEnabled = true;
                BnSend.IsEnabled = false;
                BnConnect.Content = "Connect";
                if (!_isRunning)
                    WriteToChat("Unable to connect to host server");
                _isRunning = false;
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

        private async void CheckIfRunning(object sender, CancelEventArgs e)
        {
            if (!_isRunning)
                return;
            await this.ShowMessageAsync("Don't keep me running!", "Stop or disconnect before closing this app!");
            e.Cancel = true;
        }

    }

}
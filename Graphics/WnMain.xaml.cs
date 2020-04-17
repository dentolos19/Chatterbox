using System;
using System.Net;
using System.Windows;
using Chatterbox.Core;

namespace Chatterbox.Graphics
{

    public partial class WnMain
    {

        private readonly Communicator _communicator = new Communicator();

        public WnMain()
        {
            _communicator.OnRecieved += Recieved;
            InitializeComponent();
        }

        private void WriteToChat(string message, bool newLine = true)
        {
            if (newLine)
                BxChat.Text += $"\n{message}";
            else
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
            new WnAbout().Show();
        }

        private void Host(object sender, RoutedEventArgs e)
        {
            _communicator.Host(8000);
            WriteToChat("Started hosting at port 8000", false);
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            _communicator.Connect(new IPEndPoint(IPAddress.Loopback, 8000));
            WriteToChat("Connected to host server 127.0.0.1:8000", false);
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
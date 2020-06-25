using System;
using System.Windows;
using Chatterbox.Core;
using Chatterbox.Graphics;

namespace Chatterbox
{

    public partial class App
    {

        public static Configuration Settings { get; private set; }
        public static ResourceDictionary ResourceDialog { get; private set; }

        private void Initialize(object sender, StartupEventArgs args)
        {
            Settings = Configuration.Load();
            Utilities.SetAppTheme(Utilities.GetRandomAccent());
            ResourceDialog = new ResourceDictionary { Source = new Uri("pack://application:,,,/MaterialDesignThemes.MahApps;component/Themes/MaterialDesignTheme.MahApps.Dialogs.xaml") };
            new WnMain().Show();
        }

    }
}

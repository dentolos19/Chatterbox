using System;
using System.Windows;
using Chatterbox.Core;
using Chatterbox.Graphics;

namespace Chatterbox
{

    public partial class App
    {

        internal static Configuration Settings = Configuration.Load();

        private void Initialize(object sender, StartupEventArgs e)
        {
            Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/MahApps.Metro;component/Styles/Themes/{Settings.AppTheme}.{Settings.AppAccent}.xaml")
            });
            new WnMain().Show();
        }

    }

}
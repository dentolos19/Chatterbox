using System.Windows;
using Chatterbox.Core;
using Chatterbox.Graphics;

namespace Chatterbox
{

    public partial class App
    {

        internal static readonly Configuration Settings = Configuration.Load();

        private void Initialize(object sender, StartupEventArgs e)
        {
            new WnMain().Show();
        }

    }

}
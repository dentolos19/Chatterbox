using System;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Chatterbox.Core
{

    public static class Utilities
    {

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetGetConnectedState(out int flags, int reserved);

        public static bool IsUpdateAvailable()
        {
            using var client = new WebClient();
            var data = client.DownloadString("https://raw.githubusercontent.com/dentolos19/Chatterbox/master/VERSION");
            return Version.Parse(data) < Assembly.GetExecutingAssembly().GetName().Version;
        }

        public static bool IsUserOnline()
        {
            return InternetGetConnectedState(out _, 0);
        }

        public static string GetPublicIp()
        {
            using var client = new WebClient();
            return client.DownloadString("http://ipinfo.io/ip").Replace("\n", string.Empty);
        }

    }

}
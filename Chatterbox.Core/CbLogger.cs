using System;
using System.Diagnostics;

namespace Chatterbox.Core
{

    public class CbLogger
    {

        public bool PreferDebug { get; set; }

        public void Log(string message, CbLoggerStatus status = CbLoggerStatus.Information)
        {
            var statusText = status switch
            {
                CbLoggerStatus.Information => "INFO",
                CbLoggerStatus.Warning => "WARNING",
                CbLoggerStatus.Error => "ERROR",
                _ => "UNKNOWN"
            };
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd @ HH:mm:ss} * {statusText}] {message}";
            if (PreferDebug) { Debug.WriteLine(logMessage); } else { Console.WriteLine(logMessage); }
        }

    }

}
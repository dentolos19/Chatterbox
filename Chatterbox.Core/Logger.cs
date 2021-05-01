using System;
using System.IO;

namespace Chatterbox.Core
{

    public class Logger : IDisposable
    {

        private readonly StreamWriter _writer;

        public Logger(string? outputPath = null)
        {
            if (!string.IsNullOrEmpty(outputPath))
                _writer = new StreamWriter(outputPath) { AutoFlush = true };
        }

        public void Dispose()
        {
            _writer.Close();
        }

        public void Log(string message, LoggerStatus status = LoggerStatus.Information)
        {
            var statusText = status switch
            {
                LoggerStatus.Information => "INFO",
                LoggerStatus.Warning => "WARNING",
                LoggerStatus.Error => "ERROR",
                _ => "UNKNOWN"
            };
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd @ HH:mm:ss} * {statusText}] {message}";
            Console.WriteLine(logMessage);
            _writer?.WriteLine(logMessage);
        }

    }

}
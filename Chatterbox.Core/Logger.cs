using System;
using System.Diagnostics;
using System.IO;

namespace Chatterbox.Core;

public class Logger : IDisposable
{

    private readonly StreamWriter? _writer;

    public Logger(string? outputPath = null)
    {
        if (!string.IsNullOrEmpty(outputPath))
            _writer = new StreamWriter(outputPath) { AutoFlush = true };
    }

    public void Log(string message, LoggerStatus status = LoggerStatus.Info)
    {
        var log = $"[{DateTime.Now:yyyy-MM-dd @ HH:mm:ss} * {status}] {message}";
        Debug.WriteLine(log);
        Console.WriteLine(log);
        _writer?.WriteLine(log);
    }

    public void Dispose()
    {
        _writer?.Close();
    }

}
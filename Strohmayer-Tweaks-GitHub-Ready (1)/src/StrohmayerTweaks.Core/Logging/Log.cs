namespace StrohmayerTweaks.Core.Logging;

public interface ILog
{
    void Info(string message);
    void Warn(string message);
    void Error(string message, Exception? ex = null);
}

public sealed class NullLog : ILog
{
    public void Info(string message) { }
    public void Warn(string message) { }
    public void Error(string message, Exception? ex = null) { }
}

/// <summary>Appends timestamped lines to a daily log file. Never throws.</summary>
public sealed class FileLog : ILog
{
    private readonly string _directory;
    private readonly object _lock = new();

    public FileLog(string directory)
    {
        _directory = directory;
        try { Directory.CreateDirectory(_directory); } catch { /* logging must never crash the app */ }
    }

    public void Info(string message) => Write("INFO", message);
    public void Warn(string message) => Write("WARN", message);
    public void Error(string message, Exception? ex = null) =>
        Write("ERROR", ex is null ? message : $"{message} | {ex.GetType().Name}: {ex.Message}");

    private void Write(string level, string message)
    {
        try
        {
            var file = Path.Combine(_directory, $"strohmayer-tweaks-{DateTime.Now:yyyy-MM-dd}.log");
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}";
            lock (_lock) File.AppendAllText(file, line);
        }
        catch { /* ignore */ }
    }
}

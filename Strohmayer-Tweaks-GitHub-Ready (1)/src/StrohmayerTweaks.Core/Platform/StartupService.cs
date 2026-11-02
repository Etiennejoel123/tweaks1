using Microsoft.Win32;
using StrohmayerTweaks.Core.Logging;
using StrohmayerTweaks.Core.Models;

namespace StrohmayerTweaks.Core.Platform;

public sealed record StartupEntry(string Name, string Command, RegHive Hive, bool Enabled)
{
    public string Location => Hive == RegHive.CurrentUser ? "Current user" : "All users";
}

/// <summary>Lists classic Run-key startup entries and toggles them through the same StartupApproved flag Task Manager uses (fully reversible, no entries are deleted).</summary>
public sealed class StartupService
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ApprovedKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";

    private readonly ILog _log;
    private readonly Func<bool> _isAdmin;

    public StartupService(ILog log, Func<bool> isAdmin) { _log = log; _isAdmin = isAdmin; }

    private static RegistryKey Base(RegHive h) => RegistryKey.OpenBaseKey(
        h == RegHive.CurrentUser ? RegistryHive.CurrentUser : RegistryHive.LocalMachine, RegistryView.Registry64);

    public IReadOnlyList<StartupEntry> List()
    {
        var list = new List<StartupEntry>();
        foreach (var hive in new[] { RegHive.CurrentUser, RegHive.LocalMachine })
        {
            try
            {
                using var b = Base(hive);
                using var run = b.OpenSubKey(RunKey);
                using var approved = b.OpenSubKey(ApprovedKey);
                if (run is null) continue;
                foreach (var name in run.GetValueNames())
                {
                    var cmd = run.GetValue(name)?.ToString() ?? "";
                    var enabled = true;
                    if (approved?.GetValue(name) is byte[] data && data.Length > 0) enabled = (data[0] & 1) == 0;
                    list.Add(new StartupEntry(name, cmd, hive, enabled));
                }
            }
            catch (Exception ex) { _log.Warn($"Cannot read startup entries ({hive}): {ex.Message}"); }
        }
        return list.OrderBy(e => e.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
    }

    public OperationResult SetEnabled(StartupEntry entry, bool enabled)
    {
        if (entry.Hive == RegHive.LocalMachine && !_isAdmin())
            return OperationResult.Fail("Administrator rights are required for all-users startup entries.");
        try
        {
            using var b = Base(entry.Hive);
            using var k = b.CreateSubKey(ApprovedKey, true) ?? throw new IOException("Cannot open StartupApproved key.");
            var data = new byte[12];
            if (enabled) data[0] = 2;
            else { data[0] = 3; BitConverter.GetBytes(DateTime.UtcNow.ToFileTimeUtc()).CopyTo(data, 4); }
            k.SetValue(entry.Name, data, RegistryValueKind.Binary);
            _log.Info($"Startup entry '{entry.Name}' ({entry.Hive}) set to {(enabled ? "enabled" : "disabled")}");
            return OperationResult.Ok($"'{entry.Name}' is now {(enabled ? "enabled" : "disabled")} at startup.");
        }
        catch (Exception ex) { _log.Error($"Startup toggle failed for {entry.Name}", ex); return OperationResult.Fail(ex.Message); }
    }
}

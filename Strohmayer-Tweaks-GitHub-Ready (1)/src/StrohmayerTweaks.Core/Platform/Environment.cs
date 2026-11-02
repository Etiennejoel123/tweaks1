using System.Diagnostics;
using System.Security.Principal;

namespace StrohmayerTweaks.Core.Platform;

public static class AdminHelper
{
    public static bool IsAdministrator()
    {
        if (!OperatingSystem.IsWindows()) return false;
        using var id = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator);
    }

    /// <summary>Starts a new elevated instance (UAC prompt). Returns false if the user cancelled.</summary>
    public static bool RestartAsAdministrator()
    {
        var exe = Environment.ProcessPath;
        if (exe is null) return false;
        try
        {
            Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true, Verb = "runas" });
            return true;
        }
        catch (System.ComponentModel.Win32Exception) { return false; }
    }
}

public static class Compatibility
{
    public const int Windows11MinBuild = 22000;

    public static bool IsWindows => OperatingSystem.IsWindows();
    public static bool IsWindows11 => IsWindows && Environment.OSVersion.Version.Build >= Windows11MinBuild;

    public static string Describe() => IsWindows11
        ? $"Windows 11 (build {Environment.OSVersion.Version.Build}) - supported"
        : IsWindows
            ? $"Windows build {Environment.OSVersion.Version.Build} - not Windows 11; tweaks are disabled"
            : "Not Windows - tweaks are disabled";
}

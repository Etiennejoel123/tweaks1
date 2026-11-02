using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace StrohmayerTweaks.Core.Platform;

public sealed record InfoItem(string Label, string Value);
public sealed record InfoSection(string Title, IReadOnlyList<InfoItem> Items);

public static class SystemInfoService
{
    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX status);

    private static string Reg(RegistryHive hive, string path, string name)
    {
        try
        {
            using var b = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var k = b.OpenSubKey(path);
            return k?.GetValue(name)?.ToString()?.Trim() ?? "";
        }
        catch { return ""; }
    }

    private static string Gb(ulong bytes) => $"{bytes / 1024.0 / 1024.0 / 1024.0:F1} GB";

    public static IReadOnlyList<InfoSection> Collect()
    {
        var sections = new List<InfoSection>();
        const string cv = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
        var build = Environment.OSVersion.Version.Build;

        var product = Reg(RegistryHive.LocalMachine, cv, "ProductName");
        if (build >= Compatibility.Windows11MinBuild) product = product.Replace("Windows 10", "Windows 11");
        sections.Add(new InfoSection("Operating system", new[]
        {
            new InfoItem("Edition", product.Length > 0 ? product : "Unknown"),
            new InfoItem("Version", Reg(RegistryHive.LocalMachine, cv, "DisplayVersion")),
            new InfoItem("Build", $"{build}.{Reg(RegistryHive.LocalMachine, cv, "UBR")}"),
            new InfoItem("Architecture", Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit"),
            new InfoItem("Computer name", Environment.MachineName),
            new InfoItem("User", Environment.UserName),
            new InfoItem("Uptime", FormatUptime(TimeSpan.FromMilliseconds(Environment.TickCount64))),
        }));

        var cpu = Reg(RegistryHive.LocalMachine, @"HARDWARE\DESCRIPTION\System\CentralProcessor\0", "ProcessorNameString");
        sections.Add(new InfoSection("Processor", new[]
        {
            new InfoItem("Name", cpu.Length > 0 ? cpu : "Unknown"),
            new InfoItem("Logical processors", Environment.ProcessorCount.ToString()),
        }));

        var mem = new MEMORYSTATUSEX { dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>() };
        var memItems = new List<InfoItem>();
        if (GlobalMemoryStatusEx(ref mem))
        {
            memItems.Add(new InfoItem("Total", Gb(mem.ullTotalPhys)));
            memItems.Add(new InfoItem("Available", Gb(mem.ullAvailPhys)));
            memItems.Add(new InfoItem("In use", mem.dwMemoryLoad + " %"));
        }
        else memItems.Add(new InfoItem("Total", "Unavailable"));
        sections.Add(new InfoSection("Memory", memItems));

        var gpus = new List<InfoItem>();
        const string gpuClass = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}";
        for (var i = 0; i < 6; i++)
        {
            var d = Reg(RegistryHive.LocalMachine, $@"{gpuClass}\{i:D4}", "DriverDesc");
            if (d.Length > 0) gpus.Add(new InfoItem($"Adapter {i + 1}", d));
        }
        if (gpus.Count == 0) gpus.Add(new InfoItem("Adapter", "Not detected"));
        sections.Add(new InfoSection("Graphics", gpus));

        var disks = new List<InfoItem>();
        foreach (var d in DriveInfo.GetDrives())
        {
            try
            {
                if (d.DriveType == DriveType.Fixed && d.IsReady)
                    disks.Add(new InfoItem(d.Name, $"{Gb((ulong)d.AvailableFreeSpace)} free of {Gb((ulong)d.TotalSize)} ({d.DriveFormat})"));
            }
            catch { /* skip */ }
        }
        if (disks.Count == 0) disks.Add(new InfoItem("Drives", "None detected"));
        sections.Add(new InfoSection("Storage", disks));

        return sections;
    }

    private static string FormatUptime(TimeSpan t) => $"{(int)t.TotalDays}d {t.Hours}h {t.Minutes}m";
}

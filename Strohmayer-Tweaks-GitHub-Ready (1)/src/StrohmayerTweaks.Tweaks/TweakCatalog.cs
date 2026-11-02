using StrohmayerTweaks.Core.Models;

namespace StrohmayerTweaks.Tweaks;

/// <summary>
/// Every tweak is a documented set of registry values with a known default. No scripts are downloaded or executed.
/// DWord values are unsigned decimal strings; a null default means the value does not exist on a stock system.
/// </summary>
public static class TweakCatalog
{
    private const string Adv = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
    private const string Cdm = @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager";
    private const string SysProfile = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile";

    private static RegistryValueSpec Cu(string key, string name, RegKind kind, string on, string? def) =>
        new(RegHive.CurrentUser, key, name, kind, on, def);
    private static RegistryValueSpec Lm(string key, string name, RegKind kind, string on, string? def) =>
        new(RegHive.LocalMachine, key, name, kind, on, def);

    private static Tweak T(string id, string name, string desc, TweakCategory cat, RiskLevel risk, params RegistryValueSpec[] v) =>
        new(id, name, desc, cat, risk, v);

    public static IReadOnlyList<Tweak> All { get; } = new List<Tweak>
    {
        // ---------------- Gaming ----------------
        T("gaming.gamedvr-off", "Disable Xbox Game DVR background capture",
          "Stops Windows from recording gameplay clips in the background, which can cost a few frames per second on some systems. The Xbox Game Bar itself stays available.",
          TweakCategory.Gaming, RiskLevel.Low,
          Cu(@"System\GameConfigStore", "GameDVR_Enabled", RegKind.DWord, "0", "1"),
          Cu(@"Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", RegKind.DWord, "0", "1")),
        T("gaming.game-mode", "Enable Windows Game Mode",
          "Asks Windows to prioritise the foreground game and limit background activity such as driver and update installs while you play.",
          TweakCategory.Gaming, RiskLevel.Low,
          Cu(@"Software\Microsoft\GameBar", "AutoGameModeEnabled", RegKind.DWord, "1", null),
          Cu(@"Software\Microsoft\GameBar", "AllowAutoGameMode", RegKind.DWord, "1", null)),
        T("gaming.hags", "Enable hardware-accelerated GPU scheduling",
          "Lets a supported GPU manage its own video memory scheduling. Needs a compatible GPU and recent driver; restart required. If you see stutter or crashes, revert it.",
          TweakCategory.Gaming, RiskLevel.Medium,
          Lm(@"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", RegKind.DWord, "2", null)),
        T("gaming.mmcss-games", "Raise multimedia scheduler priority for games",
          "Raises the CPU scheduling priority and category of the 'Games' multimedia task so games get CPU time ahead of background work.",
          TweakCategory.Gaming, RiskLevel.Medium,
          Lm(SysProfile + @"\Tasks\Games", "Priority", RegKind.DWord, "6", "2"),
          Lm(SysProfile + @"\Tasks\Games", "Scheduling Category", RegKind.String, "High", "Medium")),

        // ---------------- Performance ----------------
        T("perf.menu-delay", "Faster menu animations",
          "Reduces the delay before cascading menus open from 400 ms to 20 ms. Purely cosmetic responsiveness.",
          TweakCategory.Performance, RiskLevel.Low,
          Cu(@"Control Panel\Desktop", "MenuShowDelay", RegKind.String, "20", "400")),
        T("perf.transparency-off", "Disable window transparency",
          "Turns off acrylic/mica transparency effects, reducing GPU and memory use on low-end systems.",
          TweakCategory.Performance, RiskLevel.Low,
          Cu(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", RegKind.DWord, "0", "1")),
        T("perf.minimize-animation-off", "Disable window minimize/maximize animation",
          "Removes the animation when windows minimise and restore for a snappier feel.",
          TweakCategory.Performance, RiskLevel.Low,
          Cu(@"Control Panel\Desktop\WindowMetrics", "MinAnimate", RegKind.String, "0", "1")),
        T("perf.background-apps-off", "Stop Store apps running in the background",
          "Prevents Microsoft Store apps from running in the background for your account, saving memory and CPU.",
          TweakCategory.Performance, RiskLevel.Low,
          Cu(@"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", RegKind.DWord, "1", null)),
        T("perf.startup-delay-off", "Remove desktop startup delay",
          "Windows deliberately waits before launching startup apps after sign-in. This removes that delay.",
          TweakCategory.Performance, RiskLevel.Low,
          Cu(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "StartupDelayInMSec", RegKind.DWord, "0", null)),

        // ---------------- Privacy ----------------
        T("priv.ad-id", "Disable advertising ID",
          "Stops apps from using your advertising ID for personalised ads across apps.",
          TweakCategory.Privacy, RiskLevel.Low,
          Cu(@"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", RegKind.DWord, "0", "1")),
        T("priv.tailored", "Disable tailored experiences",
          "Stops Microsoft from using your diagnostic data to personalise tips, ads and recommendations.",
          TweakCategory.Privacy, RiskLevel.Low,
          Cu(@"Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", RegKind.DWord, "0", null)),
        T("priv.suggested-content", "Disable suggested content and Start suggestions",
          "Turns off promoted apps and suggestions in Settings and Start. Reverting removes the values so Windows uses its defaults again.",
          TweakCategory.Privacy, RiskLevel.Low,
          Cu(Cdm, "SubscribedContent-338388Enabled", RegKind.DWord, "0", null),
          Cu(Cdm, "SubscribedContent-338389Enabled", RegKind.DWord, "0", null),
          Cu(Cdm, "SubscribedContent-310093Enabled", RegKind.DWord, "0", null),
          Cu(Cdm, "SystemPaneSuggestionsEnabled", RegKind.DWord, "0", null)),
        T("priv.activity-history", "Disable activity history",
          "Stops Windows from collecting and publishing your activity history (Timeline-style data). Applied as a machine policy.",
          TweakCategory.Privacy, RiskLevel.Low,
          Lm(@"SOFTWARE\Policies\Microsoft\Windows\System", "PublishUserActivities", RegKind.DWord, "0", null),
          Lm(@"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", RegKind.DWord, "0", null)),
        T("priv.telemetry-required", "Limit diagnostic data to the required level",
          "Sets the diagnostic data policy to 'Required' (minimum on most editions). It does not disable the telemetry services and Windows Update keeps working.",
          TweakCategory.Privacy, RiskLevel.Low,
          Lm(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", RegKind.DWord, "1", null)),

        // ---------------- Debloat ----------------
        T("debloat.consumer-features", "Disable Windows consumer features",
          "Prevents Windows from silently installing promoted apps (for example games and trial software) on your account.",
          TweakCategory.Debloat, RiskLevel.Low,
          Lm(@"SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableWindowsConsumerFeatures", RegKind.DWord, "1", null)),
        T("debloat.widgets", "Disable the Widgets / news feed",
          "Disables the Widgets board and its background process via policy. No apps are uninstalled.",
          TweakCategory.Debloat, RiskLevel.Low,
          Lm(@"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", RegKind.DWord, "0", null)),
        T("debloat.search-suggestions", "Disable web suggestions in Start search",
          "Stops Start search from sending your typing to Bing for web suggestions; local search is unaffected.",
          TweakCategory.Debloat, RiskLevel.Low,
          Cu(@"Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", RegKind.DWord, "1", null)),
        T("debloat.lockscreen-tips", "Disable lock screen tips and promotions",
          "Removes rotating tips, fun facts and promotions from the lock screen.",
          TweakCategory.Debloat, RiskLevel.Low,
          Cu(Cdm, "RotatingLockScreenOverlayEnabled", RegKind.DWord, "0", null),
          Cu(Cdm, "SoftLandingEnabled", RegKind.DWord, "0", null)),

        // ---------------- Network ----------------
        T("net.throttling-off", "Disable multimedia network throttling",
          "Windows limits network packet processing while multimedia is playing. This removes the limit. Benefit varies by system; revert if you notice no improvement.",
          TweakCategory.Network, RiskLevel.Medium,
          Lm(SysProfile, "NetworkThrottlingIndex", RegKind.DWord, "4294967295", "10")),
        T("net.delivery-optimization-off", "Disable peer-to-peer update sharing",
          "Windows Update downloads only from Microsoft instead of also sharing update data with other PCs over your internet connection (download mode: HTTP only).",
          TweakCategory.Network, RiskLevel.Low,
          Lm(@"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", "DODownloadMode", RegKind.DWord, "0", null)),

        // ---------------- Registry (Explorer) ----------------
        T("reg.file-extensions", "Show file extensions",
          "Always shows file extensions such as .exe and .pdf, which helps spot disguised files.",
          TweakCategory.Registry, RiskLevel.Low,
          Cu(Adv, "HideFileExt", RegKind.DWord, "0", "1")),
        T("reg.hidden-files", "Show hidden files and folders",
          "Shows files and folders marked as hidden in File Explorer.",
          TweakCategory.Registry, RiskLevel.Low,
          Cu(Adv, "Hidden", RegKind.DWord, "1", "2")),
        T("reg.launch-this-pc", "Open File Explorer to 'This PC'",
          "File Explorer opens on This PC instead of Home.",
          TweakCategory.Registry, RiskLevel.Low,
          Cu(Adv, "LaunchTo", RegKind.DWord, "1", null)),
        T("reg.long-paths", "Enable long file paths",
          "Allows applications that opt in to use file paths longer than 260 characters.",
          TweakCategory.Registry, RiskLevel.Low,
          Lm(@"SYSTEM\CurrentControlSet\Control\FileSystem", "LongPathsEnabled", RegKind.DWord, "1", "0")),
    };

    public static IEnumerable<Tweak> For(TweakCategory category) => All.Where(t => t.Category == category);
}

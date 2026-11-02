using StrohmayerTweaks.Core.Models;
using StrohmayerTweaks.Core.Platform;
using StrohmayerTweaks.Tweaks;
using StrohmayerTweaks.UI.Services;

namespace StrohmayerTweaks.UI.ViewModels;

public sealed record NavItem(string Title, string Glyph, object ViewModel);

public sealed class MainViewModel : ViewModelBase
{
    private readonly AppServices _s;
    private NavItem _selected;

    public IReadOnlyList<NavItem> NavItems { get; }
    public string VersionText { get; } = "v" + (typeof(MainViewModel).Assembly.GetName().Version?.ToString(3) ?? "1.0.0");
    public bool ShowAdminBanner => !_s.IsAdmin;
    public RelayCommand RestartAsAdminCommand { get; }

    public NavItem Selected
    {
        get => _selected;
        set
        {
            if (value is null) return;
            if (Set(ref _selected, value)) (value.ViewModel as IRefreshable)?.Refresh();
        }
    }

    public MainViewModel(AppServices s)
    {
        _s = s;
        NavItems = new List<NavItem>
        {
            new("Dashboard", "\uE80F", new DashboardViewModel(s)),
            new("System Information", "\uE946", new SystemInfoViewModel()),
            new("Gaming Tweaks", "\uE7FC", new TweakPageViewModel("Gaming Tweaks", "Settings that reduce background work and favour games.", TweakCategory.Gaming, s)),
            new("Performance", "\uEC4A", new TweakPageViewModel("Performance Optimization", "Responsiveness and background-activity settings.", TweakCategory.Performance, s)),
            new("Privacy", "\uE72E", new TweakPageViewModel("Privacy", "Reduce data collection and personalised advertising.", TweakCategory.Privacy, s)),
            new("Windows Debloat", "\uE74D", new TweakPageViewModel("Windows Debloat", "Disable promotions and unwanted features. No apps are uninstalled.", TweakCategory.Debloat, s)),
            new("Network", "\uE839", new TweakPageViewModel("Network Optimization", "Network-related Windows settings.", TweakCategory.Network, s)),
            new("Services Manager", "\uE895", new ServicesViewModel(s)),
            new("Registry Tweaks", "\uE943", new TweakPageViewModel("Registry Tweaks", "File Explorer and file system registry settings.", TweakCategory.Registry, s)),
            new("Startup Manager", "\uE7E8", new StartupViewModel(s)),
            new("Restore Center", "\uE777", new RestoreViewModel(s)),
            new("Settings", "\uE713", new SettingsViewModel(s)),
        };
        _selected = NavItems[0];
        (_selected.ViewModel as IRefreshable)?.Refresh();
        RestartAsAdminCommand = new RelayCommand(() =>
        {
            if (!s.Dialogs.Confirm("Restart as administrator", "Strohmayer Tweaks will close and start again with administrator rights. Continue?")) return;
            if (AdminHelper.RestartAsAdministrator()) System.Windows.Application.Current.Shutdown();
            else s.Dialogs.Error("Elevation was cancelled.");
        });
    }
}

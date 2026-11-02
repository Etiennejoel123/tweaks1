using System.Collections.ObjectModel;
using StrohmayerTweaks.Core.Models;
using StrohmayerTweaks.Core.Platform;
using StrohmayerTweaks.Tweaks;
using StrohmayerTweaks.UI.Services;

namespace StrohmayerTweaks.UI.ViewModels;

public sealed record StatCard(string Title, string Value, string Detail);

public sealed class DashboardViewModel : ViewModelBase, IRefreshable
{
    private readonly AppServices _s;
    public ObservableCollection<StatCard> Cards { get; } = new();
    public string Compatibility { get; } = Core.Platform.Compatibility.Describe();
    public bool CompatibilityWarning => !Core.Platform.Compatibility.IsWindows11;

    public DashboardViewModel(AppServices s) => _s = s;

    public void Refresh()
    {
        Cards.Clear();
        var info = SystemInfoService.Collect();
        string Get(string section, string label) =>
            info.FirstOrDefault(i => i.Title == section)?.Items.FirstOrDefault(i => i.Label == label)?.Value ?? "Unknown";

        var applied = TweakCatalog.All.Count(t => _s.Engine.GetState(t) == TweakState.Applied);
        var backups = _s.Backups.List();

        Cards.Add(new StatCard("Operating system", Get("Operating system", "Edition"), $"Build {Get("Operating system", "Build")}"));
        Cards.Add(new StatCard("Processor", Get("Processor", "Name"), $"{Get("Processor", "Logical processors")} logical processors"));
        Cards.Add(new StatCard("Memory", Get("Memory", "Total"), $"{Get("Memory", "In use")} in use"));
        Cards.Add(new StatCard("Graphics", Get("Graphics", "Adapter 1"), "Primary adapter"));
        Cards.Add(new StatCard("Tweaks applied", $"{applied} / {TweakCatalog.All.Count}", "Detected from current registry values"));
        Cards.Add(new StatCard("Backups", backups.Count(b => b.RestoredUtc is null).ToString(), "Available in the Restore Center"));
        Cards.Add(new StatCard("Administrator", _s.IsAdmin ? "Yes" : "No", _s.IsAdmin ? "All tweaks available" : "Machine-wide tweaks are locked"));
    }
}

using System.Collections.ObjectModel;
using StrohmayerTweaks.Core.Platform;
using StrohmayerTweaks.UI.Services;

namespace StrohmayerTweaks.UI.ViewModels;

public sealed class StartupItemViewModel
{
    private readonly StartupEntry _entry;
    public StartupItemViewModel(StartupEntry entry, AppServices s, Action refresh)
    {
        _entry = entry;
        ToggleCommand = new RelayCommand(() =>
        {
            var enable = !_entry.Enabled;
            if (!s.Dialogs.Confirm("Startup entry", $"{(enable ? "Enable" : "Disable")} \"{Name}\" at startup?\n\nNothing is deleted; you can switch it back at any time.")) return;
            var r = s.Startup.SetEnabled(_entry, enable);
            if (!r.Success) s.Dialogs.Error(r.Message);
            refresh();
        });
    }
    public string Name => _entry.Name;
    public string Command => _entry.Command;
    public string Location => _entry.Location;
    public string StateText => _entry.Enabled ? "Enabled" : "Disabled";
    public string ActionText => _entry.Enabled ? "Disable" : "Enable";
    public RelayCommand ToggleCommand { get; }
}

public sealed class StartupViewModel : ViewModelBase, IRefreshable
{
    private readonly AppServices _s;
    public ObservableCollection<StartupItemViewModel> Items { get; } = new();
    public RelayCommand RefreshCommand { get; }
    public StartupViewModel(AppServices s) { _s = s; RefreshCommand = new RelayCommand(Refresh); }

    public void Refresh()
    {
        Items.Clear();
        foreach (var e in _s.Startup.List()) Items.Add(new StartupItemViewModel(e, _s, Refresh));
    }
}

using System.Collections.ObjectModel;
using StrohmayerTweaks.Core.Backup;
using StrohmayerTweaks.UI.Services;

namespace StrohmayerTweaks.UI.ViewModels;

public sealed class BackupItemViewModel
{
    private readonly BackupEntry _entry;
    public BackupItemViewModel(BackupEntry entry, AppServices s, Action refresh)
    {
        _entry = entry;
        RestoreCommand = new RelayCommand(() =>
        {
            if (!s.Dialogs.Confirm("Restore backup", $"Restore the original values saved before \"{TweakName}\" was applied?")) return;
            var r = s.Engine.Restore(_entry);
            if (r.Success) s.Dialogs.Info(r.Message); else s.Dialogs.Error(r.Message);
            refresh();
        }, () => _entry.RestoredUtc is null);
    }
    public string TweakName => _entry.TweakName;
    public string Created => _entry.CreatedUtc.ToLocalTime().ToString("g");
    public string StatusText => _entry.RestoredUtc is null ? "Available" : "Restored " + _entry.RestoredUtc.Value.ToLocalTime().ToString("g");
    public string Detail => string.Join("; ", _entry.Values.Select(v => (v.ValueName.Length == 0 ? "(Default)" : v.ValueName) + ": " + (v.Existed ? v.Data : "was not set")));
    public RelayCommand RestoreCommand { get; }
}

public sealed class RestoreViewModel : ViewModelBase, IRefreshable
{
    private readonly AppServices _s;
    public ObservableCollection<BackupItemViewModel> Items { get; } = new();
    public bool IsEmpty => Items.Count == 0;
    public RelayCommand RefreshCommand { get; }
    public RestoreViewModel(AppServices s) { _s = s; RefreshCommand = new RelayCommand(Refresh); }

    public void Refresh()
    {
        Items.Clear();
        foreach (var b in _s.Backups.List()) Items.Add(new BackupItemViewModel(b, _s, Refresh));
        Raise(nameof(IsEmpty));
    }
}

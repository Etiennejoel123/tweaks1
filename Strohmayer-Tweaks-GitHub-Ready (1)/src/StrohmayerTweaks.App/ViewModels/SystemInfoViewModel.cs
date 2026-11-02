using System.Collections.ObjectModel;
using StrohmayerTweaks.Core.Platform;

namespace StrohmayerTweaks.UI.ViewModels;

public sealed class SystemInfoViewModel : ViewModelBase, IRefreshable
{
    public ObservableCollection<InfoSection> Sections { get; } = new();
    public RelayCommand RefreshCommand { get; }

    public SystemInfoViewModel() => RefreshCommand = new RelayCommand(Refresh);

    public void Refresh()
    {
        Sections.Clear();
        foreach (var s in SystemInfoService.Collect()) Sections.Add(s);
    }
}

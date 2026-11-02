using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using StrohmayerTweaks.Core.Platform;
using StrohmayerTweaks.UI.Services;

namespace StrohmayerTweaks.UI.ViewModels;

public sealed class ServiceItemViewModel
{
    private readonly ServiceRow _row;
    public ServiceItemViewModel(ServiceRow row, AppServices s, Action refresh)
    {
        _row = row;
        StartCommand = new RelayCommand(() =>
        {
            if (!s.Dialogs.Confirm("Start service", $"Start \"{DisplayName}\"?")) return;
            var r = s.ServiceManager.Start(Name);
            if (r.Success) s.Dialogs.Info(r.Message); else s.Dialogs.Error(r.Message);
            refresh();
        }, () => Status != "Running");
        StopCommand = new RelayCommand(() =>
        {
            if (!s.Dialogs.Confirm("Stop service", $"Stop \"{DisplayName}\"?\n\nThis only stops the service for now; it is not disabled.")) return;
            var r = s.ServiceManager.Stop(Name);
            if (r.Success) s.Dialogs.Info(r.Message); else s.Dialogs.Error(r.Message);
            refresh();
        }, () => Status == "Running" && !IsProtected);
    }
    public string Name => _row.Name;
    public string DisplayName => _row.DisplayName;
    public string Status => _row.Status;
    public string StartType => _row.StartType;
    public bool IsProtected => _row.IsProtected;
    public string Summary => $"{Name}  |  {Status}  |  {StartType}";
    public RelayCommand StartCommand { get; }
    public RelayCommand StopCommand { get; }
}

public sealed class ServicesViewModel : ViewModelBase, IRefreshable
{
    private readonly AppServices _s;
    private string _search = "";
    public ObservableCollection<ServiceItemViewModel> Rows { get; } = new();
    public ICollectionView View { get; }
    public RelayCommand RefreshCommand { get; }

    public string Search
    {
        get => _search;
        set { if (Set(ref _search, value)) View.Refresh(); }
    }

    public ServicesViewModel(AppServices s)
    {
        _s = s;
        View = CollectionViewSource.GetDefaultView(Rows);
        View.Filter = o => o is ServiceItemViewModel r &&
            (string.IsNullOrWhiteSpace(_search) ||
             r.DisplayName.Contains(_search, StringComparison.CurrentCultureIgnoreCase) ||
             r.Name.Contains(_search, StringComparison.CurrentCultureIgnoreCase));
        RefreshCommand = new RelayCommand(Refresh);
    }

    public void Refresh()
    {
        Rows.Clear();
        try { foreach (var r in _s.ServiceManager.List()) Rows.Add(new ServiceItemViewModel(r, _s, Refresh)); }
        catch (Exception ex) { _s.Log.Error("Listing services failed", ex); _s.Dialogs.Error("Could not list services: " + ex.Message); }
    }
}

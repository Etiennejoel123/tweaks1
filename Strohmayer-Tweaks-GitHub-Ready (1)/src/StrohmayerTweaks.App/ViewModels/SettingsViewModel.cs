using System.Diagnostics;
using StrohmayerTweaks.Core;
using StrohmayerTweaks.Core.Platform;
using StrohmayerTweaks.UI.Services;

namespace StrohmayerTweaks.UI.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    public string AdminStatus { get; }
    public string Compatibility { get; } = Core.Platform.Compatibility.Describe();
    public string LogPath { get; } = AppPaths.Logs;
    public string BackupPath { get; } = AppPaths.Backups;
    public string RuntimeText { get; } = Environment.Version.ToString();
    public bool CanElevate { get; }
    public RelayCommand OpenLogsCommand { get; }
    public RelayCommand OpenBackupsCommand { get; }
    public RelayCommand ElevateCommand { get; }

    public SettingsViewModel(AppServices s)
    {
        AdminStatus = s.IsAdmin ? "Running as administrator" : "Running without administrator rights";
        CanElevate = !s.IsAdmin;
        OpenLogsCommand = new RelayCommand(() => Open(LogPath));
        OpenBackupsCommand = new RelayCommand(() => Open(BackupPath));
        ElevateCommand = new RelayCommand(() =>
        {
            if (!s.Dialogs.Confirm("Restart as administrator", "Strohmayer Tweaks will close and start again with administrator rights. Continue?")) return;
            if (AdminHelper.RestartAsAdministrator()) System.Windows.Application.Current.Shutdown();
            else s.Dialogs.Error("Elevation was cancelled.");
        }, () => CanElevate);
    }

    private static void Open(string path)
    {
        Directory.CreateDirectory(path);
        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"") { UseShellExecute = true });
    }
}

using System.Windows;
using StrohmayerTweaks.Core;
using StrohmayerTweaks.Core.Backup;
using StrohmayerTweaks.Core.Logging;
using StrohmayerTweaks.Core.Platform;
using StrohmayerTweaks.Core.Registry;

namespace StrohmayerTweaks.UI.Services;

public interface IDialogService
{
    bool Confirm(string title, string message);
    void Info(string message);
    void Error(string message);
}

public sealed class DialogService : IDialogService
{
    public bool Confirm(string title, string message) =>
        MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    public void Info(string message) => MessageBox.Show(message, "Strohmayer Tweaks", MessageBoxButton.OK, MessageBoxImage.Information);
    public void Error(string message) => MessageBox.Show(message, "Strohmayer Tweaks", MessageBoxButton.OK, MessageBoxImage.Warning);
}

/// <summary>Simple composition root shared by all view-models.</summary>
public sealed class AppServices
{
    public required ILog Log { get; init; }
    public required bool IsAdmin { get; init; }
    public required BackupService Backups { get; init; }
    public required TweakEngine Engine { get; init; }
    public required ServiceManagerService ServiceManager { get; init; }
    public required StartupService Startup { get; init; }
    public required IDialogService Dialogs { get; init; }

    public static AppServices Create()
    {
        var log = new FileLog(AppPaths.Logs);
        var admin = AdminHelper.IsAdministrator();
        var backups = new BackupService(AppPaths.Backups);
        log.Info($"Strohmayer Tweaks started. Admin={admin}. {Compatibility.Describe()}");
        return new AppServices
        {
            Log = log,
            IsAdmin = admin,
            Backups = backups,
            Engine = new TweakEngine(new WindowsRegistryAccessor(), backups, log, () => admin),
            ServiceManager = new ServiceManagerService(log, () => admin),
            Startup = new StartupService(log, () => admin),
            Dialogs = new DialogService()
        };
    }
}

using System.Windows;
using StrohmayerTweaks.UI.Services;
using StrohmayerTweaks.UI.ViewModels;

namespace StrohmayerTweaks.UI;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var services = AppServices.Create();

        DispatcherUnhandledException += (_, args) =>
        {
            services.Log.Error("Unhandled UI exception", args.Exception);
            MessageBox.Show("An unexpected error occurred and was logged:\n\n" + args.Exception.Message,
                "Strohmayer Tweaks", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        var window = new MainWindow { DataContext = new MainViewModel(services) };
        window.Show();
    }
}

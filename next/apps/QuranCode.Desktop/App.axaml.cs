using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using QuranCode.Desktop.Services;
using QuranCode.Desktop.ViewModels;
using QuranCode.Desktop.Views;

namespace QuranCode.Desktop;

public partial class App : Application
{
    private EngineService? _engines;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // One engine for the process, disposed when the app shuts down.
            // Constructing it opens nothing; the database is opened on first use.
            _engines = new EngineService();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(_engines),
            };
            desktop.ShutdownRequested += (_, _) => _engines.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}

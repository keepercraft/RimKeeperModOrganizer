using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using RimKeeperModOrganizerAvalonia.Views;
using System;
using System.Threading;
namespace RimKeeperModOrganizerAvalonia;

public partial class App : Application
{
    private bool _isDisposing = false;
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Program.Services.GetRequiredService<MainWindow>();
            desktop.Exit += async (s, e) =>
            {
                if (_isDisposing) return;
                _isDisposing = true;
                //tokens to cancel any ongoing operations in the app, such as background tasks or async operations
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
                {
                    await Program.AppHost.StopAsync(cts.Token);
                }
                Program.AppHost.Dispose();
                Environment.Exit(0);
            };
        }
        base.OnFrameworkInitializationCompleted();
    }
}
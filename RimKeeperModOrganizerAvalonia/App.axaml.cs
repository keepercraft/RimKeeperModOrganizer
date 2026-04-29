using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using RimKeeperModOrganizerAvalonia.Views;
using System;
namespace RimKeeperModOrganizerAvalonia;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Program.Services.GetRequiredService<MainWindow>();
//#if DEBUG
            ///desktop.MainWindow.AttachDevTools();
//#endif
        }
        //if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        //{
        //    var services = Program.Services;
        //    var mainWindow = services.GetRequiredService<MainWindow>();
        //    mainWindow.DataContext = services.GetRequiredService<MainViewModel>();
        //    desktop.MainWindow = mainWindow;
        //}
        //if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        //    desktop.MainWindow = new MainWindow();
        base.OnFrameworkInitializationCompleted();
    }
}
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RimKeeperModOrganizerAvalonia.ViewModels;
using RimKeeperModOrganizerAvalonia.Views;
using System;
namespace RimKeeperModOrganizerAvalonia;
internal class Program
{
    public static IServiceProvider Services { get; private set; } = null!;

    [STAThread]
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainWindow>();
        builder.Services.AddSingleton<RimKeeperModOrganizerLib.Services.SettingsService>();
        builder.Services.AddSingleton<RimKeeperModOrganizerLib.Services.ModsServices>();
        builder.Services.AddSingleton<RimKeeperModOrganizerLib.Services.SteamService>();       
        //builder.Services.AddSingleton<MainWindow>(sp => new MainWindow
        //{
        //    DataContext = sp.GetRequiredService<MainViewModel>()
        //});
        using IHost host = builder.Build();
        Services = host.Services;
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        //var appBuilder = BuildAvaloniaApp();
        //appBuilder.AfterSetup(b =>
        //{
        //    if (b.Instance is App app)
        //    {
        //       // app.(host.Services);
        //    }
        //});
        //appBuilder.StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
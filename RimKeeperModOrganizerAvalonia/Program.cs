using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RimKeeperModOrganizerAvalonia.Services;
using RimKeeperModOrganizerAvalonia.ViewModels;
using RimKeeperModOrganizerAvalonia.Views;
using System;
namespace RimKeeperModOrganizerAvalonia;
internal class Program
{
    public static IHost AppHost { get; private set; } = null!;
    public static IServiceProvider Services { get; private set; } = null!;

    [STAThread]
    public static void Main(string[] args)
    {
        CertificateService.EnsureCertificateInstalled();

        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddTransient<SettingsWindow>();
        builder.Services.AddSingleton<MainWindow>();
        builder.Services.AddTransient<AboutWindow>();
        builder.Services.AddSingleton<RimKeeperModOrganizerLib.Services.SettingsService>();
        builder.Services.AddSingleton<RimKeeperModOrganizerLib.Services.ModsServices>();
        builder.Services.AddSingleton<RimKeeperModOrganizerLib.Services.SteamService>();
        builder.Services.AddSingleton<ThemeService>();
        //builder.Services.AddSingleton<MainWindow>(sp => new MainWindow
        //{
        //    DataContext = sp.GetRequiredService<MainViewModel>()
        //});
        using IHost host = builder.Build();
        AppHost = host;
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
            //.ConfigureFonts(fontManager =>
            //{
            //    fontManager.AddFontCollection(new MyFontCollection());
            //})
            .LogToTrace();
}

//public sealed class MyFontCollection : EmbeddedFontCollection
//{
//    public MyFontCollection() : base(
//        new Uri("fonts:MyFonts", UriKind.Absolute),
//        new Uri("avares://MyApp/Assets/Fonts", UriKind.Absolute))
//    {
//    }
//}
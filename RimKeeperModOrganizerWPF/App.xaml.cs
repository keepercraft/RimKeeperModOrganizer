using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RimKeeperModOrganizerWPF.ViewModels;
using RimKeeperModOrganizerWPF.Views;
using System.Windows;
namespace RimKeeperModOrganizerWPF;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    //public static SettingsService SettingsService { get; } = new SettingsService();

    protected override void OnStartup(StartupEventArgs e)
    {
        var builder = Host.CreateApplicationBuilder(e.Args);
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<RimKeeperModOrganizerLib.Services.SettingsService>();
        builder.Services.AddSingleton<RimKeeperModOrganizerLib.Services.ModsServices>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<SettingsWindow>();
        var host = builder.Build();
        Services = host.Services;
        var mainWindow = new MainWindow
        {
            DataContext = Services.GetRequiredService<MainViewModel>()
        };
        mainWindow.Show();
        base.OnStartup(e);

        //Services.GetRequiredService<RimKeeperModOrganizerLib.Services.SettingsService>().StartLoad();
    }
}
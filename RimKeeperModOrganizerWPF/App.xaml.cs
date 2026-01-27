using RimKeeperModOrganizerWPF.Services;
using System.Windows;
namespace RimKeeperModOrganizerWPF;

public partial class App : Application
{
    public static SettingsService SettingsService { get; } = new SettingsService();
}
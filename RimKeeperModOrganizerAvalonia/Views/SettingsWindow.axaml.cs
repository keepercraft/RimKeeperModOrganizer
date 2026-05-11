using Avalonia.Controls;
using RimKeeperModOrganizerAvalonia.Converters;
using RimKeeperModOrganizerAvalonia.ViewModels;
namespace RimKeeperModOrganizerAvalonia.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel vm)
    {
        var ico = ModIconConverter.Get("GearIcon");
        if (ico != null)
            this.Icon = ModIconConverter.CreateIconFromDrawingImage(ico);
        InitializeComponent();
        vm.RequestClose += result => Close();
        DataContext = vm;
    }
}
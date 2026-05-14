using Avalonia.Controls;
using RimKeeperModOrganizerAvalonia.Converters;
using RimKeeperModOrganizerAvalonia.ViewModels;
namespace RimKeeperModOrganizerAvalonia.Views;

public partial class ModDetailWindow : Window
{
    public ModDetailWindow()
    {
        var ico = ModIconConverter.Get("RimworldLogoIcon");
        if (ico != null)
            this.Icon = ModIconConverter.CreateIconFromDrawingImage(ico);
        DataContextChanged += ChangeColorWindow_DataContextChanged;
        InitializeComponent();
    }

    private void ChangeColorWindow_DataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            Title = vm.SelectedMod?.About?.Name ?? "NO MOD";
            if (vm.SelectedMod != null)
            {
                var ico = ModIconConverter.GetIconKey(vm.SelectedMod.Location);
                if (ico != null)
                    this.Icon = ModIconConverter.CreateIconFromDrawingImage(ico);
            }
        }
    }

    public void OpenSelectedImageCommand(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.OpenLinkCommand.Execute(vm.SelectedModPreviewsSelection);
        }
    }

    public void OpenLinkCommand(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            if (sender is TextBlock context) vm.OpenLinkCommand.Execute(context.Text);
        }
    }

    public void ModDetailCommand(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (DataContext is MainViewModel vm && sender is TextBlock context)
        {
            vm.ModDetailCommand.Execute(vm.SelectedMod);
        }
    }
}
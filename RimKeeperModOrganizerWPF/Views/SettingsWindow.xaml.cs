using RimKeeperModOrganizerWPF.ViewModels;
using System.Windows;
namespace RimKeeperModOrganizerWPF.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel vm)
    {
        this.DataContextChanged += SettingsWindow_DataContextChanged;
        InitializeComponent();
        DataContext = vm;
    }

    private void SettingsWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
        {
            vm.RequestClose += result =>
            {
                DialogResult = result;
                Close();
            };
        }
    }
}
using RimKeeperModOrganizerLib.Models;
using System.Windows;
namespace RimKeeperModOrganizerWPF.Views;

public partial class ChangeColorWindow : Window
{
    private string? OldColor;
    public ChangeColorWindow(ModModel mod)
    {
        InitializeComponent();
        OldColor = mod?.Data?.Color;
        DataContext = mod;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        (DataContext as ModModel)?.Data?.Color = OldColor;
        DialogResult = false;
        Close();
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        (DataContext as ModModel)?.Data?.Color = null;
        DialogResult = false;
        Close();
    }
}

using RimKeeperModOrganizerWPF.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace RimKeeperModOrganizerWPF.Views;

public partial class ChangeColorWindow : Window
{
    public ChangeColorWindow(MainViewModel model)
    {
        InitializeComponent();
        DataContext = model;
        Title = model.SelectedMod?.About?.Name ?? "NO MOD";
        SetStringColor(ModColor);
    }

    private string? ModColor
    {
        get => (DataContext as MainViewModel)?.SelectedMod?.Data?.Color;
        set
        {
            if (DataContext is not MainViewModel vm) return;
            if (vm.SelectedMod?.Data == null) return;
            vm.SelectedMod.Data.Color = value;
        }
    }

    private void SetStringColor(object? value)
    {
        if (value is not string hex || string.IsNullOrWhiteSpace(hex)) return;
        try
        {
            StandardColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(hex);
        }
        catch (FormatException)
        {
            MessageBox.Show($"Nieprawidłowy kolor: {hex}");
        }
    }

    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0) return;
        SetStringColor(e.AddedItems[0]);
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        ModColor = StandardColorPicker.SelectedColor.ToString();
        DialogResult = true;
        Close();
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        ModColor = null;
        DialogResult = false;
        Close();
    }
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
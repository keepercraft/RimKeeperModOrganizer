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
        SetStringColor(ModColorActualSelected);
        BtnList.IsEnabled = !string.IsNullOrEmpty(ModColorListSelected);
    }

    public string? ModColorListSelected { get; set; } = null;

    public string? ModColorActualSelected
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
        if (e.AddedItems[0] is string c) ModColorListSelected = c;
        BtnList.IsEnabled = !string.IsNullOrEmpty(ModColorListSelected);
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        ModColorActualSelected = StandardColorPicker.SelectedColor.ToString();
        DialogResult = true;
        Close();
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        ModColorActualSelected = null;
        DialogResult = false;
        Close();
    }
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void List_Update_Click(object sender, RoutedEventArgs e)
    {
        var colorOld = ModColorListSelected;
        var colorNew = StandardColorPicker.SelectedColor.ToString();
        if (DataContext is MainViewModel vm && vm!= null && !string.IsNullOrEmpty(colorOld))
        {
            int index = vm.ModColors.IndexOf(colorOld);
            if (index >= 0)
            {
                vm.ModColors[index] = colorNew;
                ModColorListSelected = colorNew;
            }
            foreach (var item in vm.ModsConfigCollection.Union(vm.ModsCollection).Where(w => w.Data != null && w.Data?.Color == colorOld))
            {
                item.Data.Color = colorNew;
            }
        }
    }

    private void List_Delete_Click(object sender, RoutedEventArgs e)
    {
        var colorOld = ModColorListSelected;
        if (DataContext is MainViewModel vm && vm != null && !string.IsNullOrEmpty(colorOld))
        {         
            int index = vm.ModColors.IndexOf(colorOld);
            if (index >= 0)
            {
                vm.ModColors.RemoveAt(index);
                ModColorListSelected = null;
                BtnList.IsEnabled = false;
            }
            foreach (var item in vm.ModsConfigCollection.Union(vm.ModsCollection).Where(w => w.Data != null && w.Data?.Color == colorOld))
            {
                item.Data.Color = null;
            }
        }
    }
}
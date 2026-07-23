using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorPicker;
using RimKeeperModOrganizerAvalonia.Converters;
using RimKeeperModOrganizerAvalonia.Extensions;
using RimKeeperModOrganizerAvalonia.ViewModels;
using RimKeeperModOrganizerLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
namespace RimKeeperModOrganizerAvalonia.Views;

public partial class ChangeColorWindow : Window
{
    public ChangeColorWindow()
    {
        var ico = ModIconConverter.Get("PaletteIcon");
        if (ico != null)
            this.Icon = ModIconConverter.CreateIconFromDrawingImage(ico);
        DataContextChanged += ChangeColorWindow_DataContextChanged;
        InitializeComponent();
    }

    private void ChangeColorWindow_DataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            Title = vm.SelectedMods.Count > 1 ? $"({vm.SelectedMods.Count}) Mods Selected" : vm.SelectedMod?.About?.Name ?? "NO MOD";
            SetStringColor(ModColorActualSelected);
            UpdateColor(vm);
            BtnList.IsEnabled = !string.IsNullOrEmpty(ModColorListSelected);
        }
    }

    public string? ModColorListSelected { get; set; } = null;

    public string? ModColorActualSelected
    {
        get
        {
            if (DataContext is not MainViewModel vm) return null;
            if (vm.SelectedMods != null && vm.SelectedMods.Count > 1)
            {
                var color = vm.SelectedMods
                    .GroupBy(s => s?.Data?.Color)
                    .OrderByDescending(s => s.Count())
                    .Select(s => s.Key)
                    .Where(s => s != null)
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(color)) return color;
            }
            return vm.SelectedMod?.Data?.Color; //return (DataContext as MainViewModel)?.SelectedMod?.Data?.Color;
        }
        set
        {
            if (DataContext is not MainViewModel vm) return;
            if (vm.SelectedMods != null)
            {
                foreach (var item in vm.SelectedMods)
                {
                    if (item == null) continue;
                    if (item?.Data == null) item?.MakeData();
                    item.Data.Color = value;
                }
            }
            else //maybe never happens, but just in case
            {
                if (vm.SelectedMod?.Data == null) vm.SelectedMod?.MakeData();
                vm.SelectedMod?.Data?.Color = value;
            }
            vm.SelectedMod?.RaisePropertyChanged();
            UpdateColor(vm);
        }
    }

    private void SetStringColor(object? value)
    {
        if (value is not string hex || string.IsNullOrWhiteSpace(hex)) return;
        try
        {
            var color = Avalonia.Media.Color.Parse(hex);
            StandardColorPicker.SelectedColor = color;
            //StandardColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(hex);
        }
        catch (FormatException)
        {
           // MessageBox.Show($"Nieprawidłowy kolor: {hex}");
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
        ModColorActualSelected = StandardColorPicker.SelectedColor.ToRGBString();
       // DialogResult = true;
        Close();
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        ModColorActualSelected = null;
       // DialogResult = false;
        Close();
    }
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
       // DialogResult = false;
        Close();
    }

    private void List_Update_Click(object sender, RoutedEventArgs e)
    {
        var colorOld = ModColorListSelected;
        var colorNew = StandardColorPicker.SelectedColor.ToRGBString();
        if (DataContext is MainViewModel vm && vm != null && !string.IsNullOrEmpty(colorOld))
        {
            int index = vm.ModColors.IndexOf(colorOld);
            if (index >= 0)
            {
                vm.ModColors[index] = colorNew;
                ModColorListSelected = colorNew;
            }
            foreach (var item in vm.Items.Where(w => w.Data != null && w.Data?.Color == colorOld))
            {
                item.Data.Color = colorNew;
            }
            for (int i = 0; i < vm.ModColors.Count; i++)
            {
                if (i == index) continue;
                if (vm.ModColors[i] == colorNew)
                {
                    vm.ModColors.RemoveAt(i);
                }
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
            foreach (var item in vm.Items.Where(w => w.Data != null && w.Data?.Color == colorOld))
            {
                item.Data.Color = null;
            }
        }
    }

    private bool UpdateColor(MainViewModel vm)
    {
        IEnumerable<string> items = vm.Items
            .Where(x => x.Data != null && x.Data.IsNotNull())
            .Select(x => x.Data.Color)
            .Where(color => !string.IsNullOrEmpty(color))
            .Distinct();
        var b = vm.ModColors.SyncWith<string>(items);
        if (b) vm.RaisePropertyChanged(nameof(MainViewModel.ModColorIconsList));
        return b;
    }
}
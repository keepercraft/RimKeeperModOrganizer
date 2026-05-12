using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.VisualTree;
using RimKeeperModOrganizerAvalonia.Converters;
using RimKeeperModOrganizerAvalonia.ViewModels;
using RimKeeperModOrganizerLib.Extensions;
using RimKeeperModOrganizerLib.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
namespace RimKeeperModOrganizerAvalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow() 
    {
        InitializeComponent();
        //AvaloniaXamlLoader.Load(this);
    }
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        //AvaloniaXamlLoader.Load(this);
        DataContext = viewModel;

        ModsGrid.AddHandler(DragDrop.DragOverEvent, OnDragOver);
        ModsGrid.AddHandler(DragDrop.DropEvent, OnDrop);
        ModsGrid.AddHandler(PointerPressedEvent, DataGrid_PointerPressed, RoutingStrategies.Tunnel);
        ModsGrid.AddHandler(PointerMovedEvent, DataGrid_PointerMoved, RoutingStrategies.Tunnel);
        ModsGrid.AddHandler(PointerReleasedEvent, DataGrid_PointerReleased, RoutingStrategies.Tunnel);
        ModsGridConfig.AddHandler(DragDrop.DragOverEvent, OnDragOver);
        ModsGridConfig.AddHandler(DragDrop.DropEvent, OnDrop);
        ModsGridConfig.AddHandler(PointerPressedEvent, DataGrid_PointerPressed, RoutingStrategies.Tunnel);
        ModsGridConfig.AddHandler(PointerMovedEvent, DataGrid_PointerMoved, RoutingStrategies.Tunnel);
        ModsGridConfig.AddHandler(PointerReleasedEvent, DataGrid_PointerReleased, RoutingStrategies.Tunnel);

        //DragDrop.AddDropHandler(ModsGrid, OnDrop);
        //DragDrop.AddDragOverHandler(ModsGrid, OnDragOver);
        //DragDrop.AddDropHandler(ModsGridConfig, OnDrop);
        //DragDrop.AddDragOverHandler(ModsGridConfig, OnDragOver);
        //DragDrop.AddDragLeaveHandler(ModsGrid, OnDrop2);
        //DragDrop.AddDragLeaveHandler(ModsGridConfig, OnDrop2);

        //ModsGrid.LoadingRow += OnLoadingRow;
    }

    private void ToggleThemeClick(object? sender, RoutedEventArgs e)
    {
        Application.Current!.RequestedThemeVariant = 
            Application.Current!.RequestedThemeVariant == ThemeVariant.Light
                ? ThemeVariant.Dark
                : ThemeVariant.Light;
        var content = Content;
        Content = null;
        Content = content;
        InvalidateVisual();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ContentProperty)
        {
            var ico = ModIconConverter.Get("RimworldLogoIcon");
            if (ico != null)
                this.Icon = ModIconConverter.CreateIconFromDrawingImage(ico);
        }

        var property = change.Property.Name;
        var newvalue = change.NewValue;
        Debug.WriteLine(">>"+ property+" - "+ newvalue + " :::: " + this.Position.X +":"+ this.Position.Y);
    }

    public void OpenLinkCommand(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (DataContext is MainViewModel vm && sender is TextBlock context)
        {
            vm.OpenLinkCommand.Execute(context.Text);
        }
    }

    public void ModDetailCommand(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (DataContext is MainViewModel vm && sender is TextBlock context)
        {
            vm.ModDetailCommand.Execute(vm.SelectedMod);
        }
    }

    private Point _dragStartPoint;
    private bool _isReadyToDrag;
    private PointerPressedEventArgs? _lastPressedArgs;
    private IList _lastSelectedItmes;
    private Visual _lastSelectedsource;
    private static readonly DataFormat<object> RowDragFormat = DataFormat.CreateInProcessFormat<object>("app/row-index");
    public async void DataGrid_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var pointerProps = e.GetCurrentPoint(this).Properties;

        if (pointerProps.IsLeftButtonPressed)
        {
            if(_lastPressedArgs == null)
            {
                List<object> list = new List<object>();
                foreach (var item in (sender as DataGrid)?.SelectedItems)
                {
                    list.Add(item);
                }
                _lastSelectedItmes = list;
                _lastSelectedsource = e.Source as Visual;

                //_lastSelectedItmes = (sender as DataGrid)?.SelectedItems.;
            }
            
            // ZAPISUJEMY CAŁE ARGUMENTY
            _lastPressedArgs = e;
            _dragStartPoint = e.GetPosition(this);
        }
        /*
        try
        {
            if (sender is not DataGrid dataGrid) return;
            var visual = e.Source as Visual;
            var row = visual?.FindAncestorOfType<DataGridRow>();
            if (row == null) return;

            var pointerProperties = e.GetCurrentPoint(row).Properties;
            if (!pointerProperties.IsLeftButtonPressed) return;

            object dragData;
            if (dataGrid.SelectedItems.Count > 1 && dataGrid.SelectedItems.Contains(row.DataContext))
            {
                dragData = dataGrid.SelectedItems.Cast<object>().ToList();
            }
            else
            {
                dragData = row.DataContext!;
            }

            var item = new DataTransferItem();
            item.Set(RowDragFormat, dragData);
            var data = new DataTransfer();
            data.Add(item);
            await DragDrop.DoDragDropAsync(e, data, DragDropEffects.Move);
        }
        catch (Exception ex)
        { 
        }
        */
    }
    public async void DataGrid_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_lastPressedArgs == null) return;
        var currentPoint = e.GetPosition(this);
        var delta = _dragStartPoint - currentPoint;

        if (Math.Abs(delta.X) > 5 || Math.Abs(delta.Y) > 5)
        {
            // Pobieramy dane do przeciągnięcia
            //var visual = e.Source as Visual;
           // var datagrid = visual?.FindAncestorOfType<DataGrid>();
            var items2 = _lastSelectedItmes;
            //var items = datagrid.SelectedItems;
            //var row = visual?.FindAncestorOfType<DataGridRow>();
            //var draggedData = row?.DataContext;
            if (items2.Count == 0)
            {
                List<object> list = new List<object>();
                foreach (var item in (sender as DataGrid)?.SelectedItems)
                {
                    items2.Add(item);
                }
            }
                //if (draggedData != null)
            if (items2.Count > 0)
            {
                var item = new DataTransferItem();
                item.Set(RowDragFormat, items2);

                var data = new DataTransfer();
                data.Add(item);

                // UŻYWAMY ZAPISANYCH ARGUMENTÓW Z POINTERPRESSED
                var triggerArgs = _lastPressedArgs;
                _lastPressedArgs = null; // Czyścimy, aby nie odpalić dwa razy

                await DragDrop.DoDragDropAsync(triggerArgs, data, DragDropEffects.Move);
            }
        }
    }
    public void DataGrid_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _lastSelectedItmes?.Clear();
        _lastPressedArgs = null;
        //_isReadyToDrag = false;
        ClearDragHighlight();
    }
    private void OnDragOver(object? sender, DragEventArgs e)
    {
        //if (e.DataTransfer.Contains(RowDragFormat))
        //{
        //    e.DragEffects = DragDropEffects.Move;
        //}
        //else
        //{
        //    e.DragEffects = DragDropEffects.None;
        //}
        //e.Handled = true;
        if (!e.DataTransfer.Items.Any(i => i.Formats.Contains(RowDragFormat)))
        {
            e.DragEffects = DragDropEffects.None;
            return;
        }

        e.DragEffects = DragDropEffects.Move;

        var visual = e.Source as Visual;
        var row = visual?.FindAncestorOfType<DataGridRow>();

        // Jeśli zmieniliśmy wiersz, czyścimy stary
        if (_highlightedRow != null && _highlightedRow != row)
        {
            ClearDragHighlight();
        }

        if (row != null)
        {
            _highlightedRow = row;

            // Obliczamy, czy mysz jest w górnej, czy dolnej połowie wiersza
            var position = e.GetPosition(row);
            bool isTopHalf = position.Y < (row.Bounds.Height / 2);

            if (isTopHalf)
            {
                row.Classes.Add("insert-top");
                row.Classes.Remove("insert-bottom");
            }
            else
            {
                row.Classes.Add("insert-bottom");
                row.Classes.Remove("insert-top");
            }
        }
    }
    public void OnDrop(object? sender, DragEventArgs e)
    {
        if (sender is not DataGrid dataGrid) return;
        var item = e.DataTransfer.Items.FirstOrDefault(i => i.Formats.Contains(RowDragFormat));
        var droppedData = item?.TryGetRaw(RowDragFormat);
        if (droppedData == null) return;

        // 2. Wyznaczenie miejsca docelowego
        var visual = e.Source as Visual;
        var targetDataGrid = visual?.FindAncestorOfType<DataGrid>();
        var targetRowTarget = visual?.FindAncestorOfType<DataGridRow>();
        var position = e.GetPosition(targetRowTarget);
        bool isTopHalf = position.Y < (targetRowTarget.Bounds.Height / 2);

        // Jeśli nie upuszczono na konkretny wiersz, wstawiamy na koniec
        int targetIndex = targetRowTarget != null
            ? targetRowTarget.GetIndex()
            : dataGrid.ItemsSource.Cast<object>().Count();



        // 3. Sprawdzenie, czy źródło danych pozwala na modyfikację
        if (dataGrid.ItemsSource is not IList itemsSource) return;

        bool isDroppingToAssigned = targetDataGrid.Name == "ModsGridConfig";
        // 4. Normalizacja danych (obsługa pojedynczego elementu i listy zaznaczenia)
        var itemsToMove = droppedData is IList list
            ? list.Cast<ModModel>().ToList()
            : new List<ModModel>();

        if(this.DataContext is not MainViewModel vm) return;

        if (targetRowTarget.DataContext is ModModel targetItem)
             targetIndex = vm.Items.IndexOf(targetItem);

        // 5. Proces przenoszenia
        if (!itemsToMove.Any()) return;
        var item_first_index = vm.Items.IndexOf(itemsToMove.First());
        if (item_first_index <= targetIndex && item_first_index + itemsToMove.Count >= targetIndex)
        {
            return;
        }
        foreach (var toMove in itemsToMove)
        {

            if (!isDroppingToAssigned) toMove.Position = null; else toMove.Position = 0;
            int currentIndex = vm.Items.IndexOf(toMove);
            if (currentIndex == -1) continue;
            int actualTarget = currentIndex < targetIndex ? targetIndex - 1 : targetIndex;
            if (!isTopHalf)
            {
                actualTarget++;
            }
            actualTarget = Math.Max(0, Math.Min(actualTarget, vm.Items.Count() - 1));
            //itemsSourceModels.Move(currentIndex, actualTarget);
            vm.Items.Move(currentIndex, actualTarget);
            //if (currentIndex == targetRowIndex) return;

            int i = 0;
            var ttt = vm.ModsConfigCollection.Cast<ModModel>().Where(w => w.Position != null);
            foreach (var item22 in ttt)
            {
                item22.Position = vm.Items.IndexOf(item22);
            }



            //if (oldIndex != -1)
            //{
            //    // Jeśli element już jest w kolekcji, usuwamy go z poprzedniej pozycji
            //    itemsSource.RemoveAt(oldIndex);

            //    // Jeśli usuwany element był przed miejscem docelowym, musimy cofnąć targetIndex
            //    if (oldIndex < targetIndex)
            //    {
            //        targetIndex--;
            //    }
            //}

            // Zabezpieczenie przed wyjściem poza zakres i wstawienie
            //targetIndex = Math.Clamp(targetIndex, 0, itemsSource.Count);
            //itemsSource.Insert(targetIndex, toMove);

            //// Inkrementacja, aby kolejne elementy z paczki wskakiwały jeden pod drugim
            //targetIndex++;
        }

        _lastSelectedItmes.Clear();
        _lastPressedArgs = null;
        //_isReadyToDrag = false;
        ClearDragHighlight();

        vm.ModsConfigCollection.Refresh();
        vm.ModsCollection.Refresh();

        vm.Items.ModListAlertClean();
        vm.ModsConfigCollection.Cast<ModModel>().ModListValidation(vm.GameVersion);

        vm.AlertPropertyChanged();
    }
    private DataGridRow? _highlightedRow;

    private void ClearDragHighlight()
    {
        if (_highlightedRow != null)
        {
            _highlightedRow.Classes.Remove("insert-top");
            _highlightedRow.Classes.Remove("insert-bottom");
            _highlightedRow = null;
        }
    }
}
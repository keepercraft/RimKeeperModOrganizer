using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Microsoft.Extensions.Hosting;
using RimKeeperModOrganizerAvalonia.Converters;
using RimKeeperModOrganizerAvalonia.ViewModels;
using RimKeeperModOrganizerLib.Extensions;
using RimKeeperModOrganizerLib.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
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
        /* Drag and Drop OLD FUNCTIONALITY
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
        this.PointerMoved += MainWindow_PointerMoved;
        */

        //ModsGrid.SelectionChanged += DataGrid_OnSelectionChanged;
        //ModsGrid.PointerPressed += DataGrid_OnPointerPressed;
        //AddHandler(PointerPressedSelectionEvent, OnPointerReleased2, RoutingStrategies.Tunnel);

        ModsGrid.AddHandler(KeeperDataGridAvalonia.KeeperDataGrid.PointerPressedSelectionEvent, DataGrid_PointerPressedSelection, RoutingStrategies.Tunnel);
        ModsGridConfig.AddHandler(KeeperDataGridAvalonia.KeeperDataGrid.PointerPressedSelectionEvent, DataGrid_PointerPressedSelection, RoutingStrategies.Tunnel);
        ModsGrid.AddHandler(PointerReleasedEvent, DataGrid_PointerReleasedSelection, RoutingStrategies.Tunnel);
        ModsGridConfig.AddHandler(PointerReleasedEvent, DataGrid_PointerReleasedSelection, RoutingStrategies.Tunnel);
        ModsGrid.AddHandler(PointerMovedEvent, DataGrid_Popup_PointerMoved, RoutingStrategies.Tunnel);
        ModsGridConfig.AddHandler(PointerMovedEvent, DataGrid_Popup_PointerMoved, RoutingStrategies.Tunnel);
        //this.PointerMoved += DataGrid_Popup_PointerMoved;
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
        //Debug.WriteLine(">>"+ property+" - "+ newvalue + " :::: " + this.Position.X +":"+ this.Position.Y);
    }

    #region CLICK
    private void ToggleThemeClick(object? sender, RoutedEventArgs e)
    {
        Application.Current!.RequestedThemeVariant =
            Application.Current!.RequestedThemeVariant == ThemeVariant.Light
                ? ThemeVariant.Dark
                : ThemeVariant.Light;
        //var content = Content;
        //Content = null;
        //Content = content;
        //InvalidateVisual();
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
    #endregion

    #region Drag and Drop 
    private async void DataGrid_PointerPressedSelection(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not DataGrid context) return;
        //if (e.Source is not Control contextSource) return;
        //_drag_Popup_Start_Source = context;
        _drag_Popup_Start_Position = e.GetPosition(this);
        //var items = _drag_Popup_list = context.SelectedItems;
        //Drag_Popup(this, items);
        //var dragData = new DataTransfer();
        //dragData.Add(DataTransferItem.CreateText("Hello from drag!"));
        //await DragDrop.DoDragDropAsync(e, dragData, DragDropEffects.None);
        Debug.WriteLine("DataGrid_OnPointerPressed> items:" + context.SelectedItems.Count);
    }
    public async void DataGrid_Popup_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (sender is not DataGrid drag_source) return;
        if (_drag_Popup_Start_Position == null) return;
        if (_drag_Popup != null)
        {
            var tt = (sender as DataGrid);
            var currentPoint = e.GetPosition(this);
            _drag_Popup.HorizontalOffset = currentPoint.X + 5;
            _drag_Popup.VerticalOffset = currentPoint.Y + 15 + _drag_Popup_Height;
            Debug.WriteLine("GetPosition X:" + _drag_Popup.HorizontalOffset + " Y:" + _drag_Popup.VerticalOffset + " - "+ tt?.Name??"?");
            //if (e.Source is not Control contextSource) return;
            if (Drag_Row_Highlight(sender, e, ModsGrid)) return;
            if (Drag_Row_Highlight(sender, e, ModsGridConfig)) return;
            //Debug.WriteLine("DataGrid_Popup_PointerMoved:" + contextSource.Name);
        }
        else
        {
            var currentPoint = e.GetPosition(this);
            var x = currentPoint.X - _drag_Popup_Start_Position?.X ?? currentPoint.X;
            var y = currentPoint.Y - _drag_Popup_Start_Position?.Y ?? currentPoint.Y;
            //Debug.WriteLine("GetPosition X:" + x + " Y:" + y);
            if (Math.Abs(x) > _drag_Popup_Start_Position_Delta || Math.Abs(y) > _drag_Popup_Start_Position_Delta)
            {
                var items = drag_source.SelectedItems;
                if ((items?.Count ?? 0) > 0)
                {
                    _drag_Popup_list = items.Cast<ModModel>().ToList();
                    Drag_Popup(this, items);
                }
            }
        }
    }
    private void DataGrid_PointerReleasedSelection(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is not DataGrid drag_source) return;
        if (_drag_Popup != null)
        {
            DataGrid? drag_target = _drag_Popup_Row?.FindAncestorOfType<DataGrid>();
            if (drag_target == null) return;

          //  var itemsSource = _drag_Popup_Start_Source.ItemsSource;
          //  var itemsTarget = drag_target.ItemsSource;
            var itemsSourceObservable = GetObservableSorce<ModModel>(drag_source);
            var itemsTargetObservable = GetObservableSorce<ModModel>(drag_target);
            //if (itemsSourceObservable != itemsTargetObservable) return;

            var offset = _drag_Popup_Row_Offset;
            if (_drag_Popup_Row.DataContext is not ModModel itemTarget) return;
            foreach (var item in _drag_Popup_list.Cast<ModModel>())
            {
                var itemIndexSource = itemsSourceObservable.IndexOf(item);
                var itemIndexTarget = itemsTargetObservable.IndexOf(itemTarget);
                int index = itemIndexSource < itemIndexTarget ? itemIndexTarget - 1 : itemIndexTarget;
                if (!offset && itemIndexSource != itemIndexTarget) index++;
                item.Position = drag_target == ModsGridConfig ? index : null;
                itemsTargetObservable.Move(itemIndexSource, index);
                //Debug.WriteLine($"MOVE:{itemIndexSource}->{itemIndexTarget}+{offset}");
            }
            if(drag_target == ModsGridConfig || drag_source == ModsGridConfig)
            {
                var list = GetObservableSorce<ModModel>(ModsGridConfig);
                foreach (var item in list.Where(w => w.Position != null))
                {
                    item.Position = list.IndexOf(item);
                }
            }
            (drag_target.ItemsSource as IDataGridCollectionView)?.Refresh();
            (drag_source.ItemsSource as IDataGridCollectionView)?.Refresh();

            if (this.DataContext is MainViewModel vm)
            {
                vm.ModsCollection.Cast<ModModel>().ModListAlertClean();
                vm.Items.ModListDuplicateValidation();
                vm.ModsConfigCollection.Cast<ModModel>().ModListValidation(vm.GameVersion);
                vm.AlertPropertyChanged();
            }
        }
        Drag_Row_Highlight_Remove(drag_source);
        Drag_Popup_Close();
        _drag_Popup_Start_Position = null;
    }

    public ObservableCollection<T>? GetObservableSorce<T>(DataGrid grid)
    {
        if (grid.ItemsSource is ObservableCollection<T> observable) return observable;
        if (ModsGrid.ItemsSource is IDataGridCollectionView view)
        {
            if (view.SourceCollection is ObservableCollection<T> sourceList) return sourceList;
        }
        if (grid.ItemsSource is IEnumerable<T> enumerable)
        {
            var newObservable = new ObservableCollection<T>(enumerable);
            grid.ItemsSource = newObservable;
            return newObservable;
        }
        return null;
    }

    public Border _rowSeparator = new Border
    {
        Height = 2,
        Background = Brushes.DeepSkyBlue, // Kolor podświetlenia
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
        IsHitTestVisible = false,
        ZIndex = 1000
    };
    private void Drag_Row_Highlight_Remove(Visual visual)
    {
        var layer = AdornerLayer.GetAdornerLayer(visual);
        layer?.Children.Remove(_rowSeparator);
    }
    public bool Drag_Row_Highlight(object? sender, PointerEventArgs e, DataGrid context)
    {
        var pointInGrid = e.GetPosition(context);
        var visualUnderCursor = context.InputHitTest(pointInGrid);
        if (visualUnderCursor is Control contextSource)
        {
            var row = contextSource.FindAncestorOfType<DataGridRow>();
            var adornerLayer = AdornerLayer.GetAdornerLayer(context);
            if (row != null && adornerLayer != null)
            {
                if (!adornerLayer.Children.Contains(_rowSeparator))
                    adornerLayer.Children.Add(_rowSeparator);

                _rowSeparator.IsVisible = true;

                var rowTopLeftInAdorner = row.TranslatePoint(new Point(0, 0), adornerLayer);
                if (rowTopLeftInAdorner.HasValue)
                {
                    double rowWidth = row.Bounds.Width;
                    double rowHeight = row.Bounds.Height;

                    // 2. Sprawdzamy pozycję kursora względem wiersza
                    var pointInRow = e.GetPosition(row);

                    var offset = pointInRow.Y < rowHeight / 2;
                    // 3. Decydujemy czy góra, czy dół wiersza
                    double targetY = offset
                                     ? rowTopLeftInAdorner.Value.Y
                                     : rowTopLeftInAdorner.Value.Y + rowHeight;

                    // 4. Ustawiamy szerokość i pozycję
                    _rowSeparator.Width = rowWidth;

                    // Przesuwamy linię dokładnie tam, gdzie zaczyna się wiersz w Adornerze
                    _rowSeparator.RenderTransform = new TranslateTransform(
                        rowTopLeftInAdorner.Value.X,
                        targetY - (_rowSeparator.Height / 2));

                    _drag_Popup_Row = row;
                    _drag_Popup_Row_Offset = offset;
                    return true;
                }
            }
        }
        else
        {
            _rowSeparator.IsVisible = false;
        }
        return false;
    }

    private DataGridRow _drag_Popup_Row;
    private bool _drag_Popup_Row_Offset;
    private IList? _drag_Popup_list;
    private double _drag_Popup_Height;
    private Popup? _drag_Popup;
    private double _drag_Popup_Start_Position_Delta = 10;
    private Point? _drag_Popup_Start_Position;
    public void Drag_Popup_Close()
    {
        if (_drag_Popup != null)
        {
            _drag_Popup.Close();
            _drag_Popup = null;
        }
    }
    public Popup Drag_Popup(Control? control, IList items)
    {
        Drag_Popup_Close();
        var sp = new StackPanel()
        {
            Background = Brushes.Transparent,
        };
        int i = 0;
        foreach (var item in items.Cast<ModModel>().Select(s => s.Label))
        {
            if (i > 10) break;
            sp.Children.Add(new TextBlock
            {
                Text = i == 10 ? $"... {items.Count-10}" : item,
                Background = Brushes.Transparent,
                //Foreground = Brushes.LightGray,
            });
            if (i < items.Count - 1 && i < 9)
            {
                sp.Children.Add(new Rectangle
                {
                    Height = 1,
                    Fill = Brushes.Gray,
                    Margin = new Thickness(0, 1),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
                });
            }
            i++;
        }
        var popup = new Popup
        {
            IsHitTestVisible = false,
            PlacementTarget = control,
            Placement = PlacementMode.TopEdgeAlignedLeft,
            Child = new Border
            {
                Padding = new Thickness(0),
                Margin = new Thickness(2),
                CornerRadius = new CornerRadius(3),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Background = Brushes.Black,
                Opacity = 0.9,
                Child = sp,
            }
        };
        this.LogicalChildren.Add(popup);
        //popup.PointerMoved += DataGrid_Popup_PointerMoved;
        popup.IsOpen = true;
        _drag_Popup_Height = sp.DesiredSize.Height;
        return _drag_Popup = popup;
    }

    #endregion

    #region Drag and Drop OLD
    private Popup? _dragPopup;
    private void MainWindow_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragPopup?.IsOpen == true)
        {
            var currentPoint = e.GetPosition(this);
            //_dragPopup.PlacementTarget = this;
            //_dragPopup.Placement = PlacementMode.LeftEdgeAlignedTop;
            _dragPopup.HorizontalOffset = currentPoint.X + 10;
            _dragPopup.VerticalOffset = currentPoint.Y + 10;
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

                if (_dragPopup != null)
                {
                    _dragPopup?.Close();
                    _dragPopup = null;
                }
                var sp = new StackPanel();
                foreach (var item in list.Cast<ModModel>().Select(s => s.Label))
                {
                    sp.Children.Add(new TextBlock
                    {
                        Text = item,
                        Foreground = Brushes.White,
                        Margin = new Thickness(2)
                    });
                }
                //var popup = new Canvas
                //{
                //    Height = 200,
                //    Width = 200,
                //    IsHitTestVisible = false, // Cały canvas nie reaguje na mysz
                //    ZIndex = 1000, // Zawsze nad resztą kontrolek
                //    Children = { new Border
                //        {
                //            Background = Brushes.Black,
                //            Child = sp,
                //        }
                //    }
                //};
                var popup = new Popup
                {
                    IsEnabled = false,
                    IsHitTestVisible = false,
                    PlacementTarget = this,
                    Placement = PlacementMode.LeftEdgeAlignedTop,
                    Child = new Border
                    {

                        Background = Brushes.Black,
                        Child = sp,
                    }
                };
                popup.IsOpen = true;
                _dragPopup = popup;
                MainGrid.Children.Add(popup);
                /*
                                //_lastSelectedItmes = (sender as DataGrid)?.SelectedItems.;
                                _dragPreview = new Border
                                {
                                    Background = Brushes.Black,
                                    CornerRadius = new CornerRadius(4),
                                    Padding = new Thickness(6),
                                    Child = new ItemsControl
                                    {
                                        Foreground = Brushes.White,
                                        ItemsSource = list.Cast<ModModel>().Select(s => s.Label).ToList()
                                    }
                                };
                                _dragPopup = new Popup
                                {
                                    PlacementTarget = sender as DataGrid,
                                    Placement = PlacementMode.Pointer,
                                    IsHitTestVisible = false,
                                    IsOpen = true,
                                    Child = _dragPreview
                                };

                                var currentPoint = e.GetPosition(this);
                                _dragPopup?.HorizontalOffset = currentPoint.X + 10;
                                _dragPopup?.VerticalOffset = currentPoint.Y + 10;
                 */
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
        var currentPoint = e.GetPosition(this);

        //Debug.WriteLine("DragPopup:" + ((_dragPopup?.IsOpen??false)?"open":"null"));
        //_dragPopup?.HorizontalOffset = currentPoint.X + 10;
        //_dragPopup?.VerticalOffset = currentPoint.Y + 10;

        if (_lastPressedArgs == null) return;        
        var delta = _dragStartPoint - currentPoint;
        if (Math.Abs(delta.X) > 5 || Math.Abs(delta.Y) > 5)
        {
            try
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
            catch
            {
            }
        }
    }
    public void DataGrid_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _dragPopup?.Close();
        //_dragPopup.Children.Clear();
        _dragPopup = null;

        _lastSelectedItmes?.Clear();
        _lastPressedArgs = null;
        //_isReadyToDrag = false;
        ClearDragHighlight();
    }
    private void OnDragOver(object? sender, DragEventArgs e)
    {
        if (_dragPopup != null)
        {
            var currentPoint = e.GetPosition(this);
            //_dragPopup.PlacementTarget = this;
            //_dragPopup.Placement = PlacementMode.LeftEdgeAlignedTop;
            _dragPopup.HorizontalOffset = currentPoint.X + 0;
            _dragPopup.VerticalOffset = currentPoint.Y + 0;
            //Canvas.SetLeft(_dragPopup, currentPoint.X + 0);
            //Canvas.SetTop(_dragPopup, currentPoint.Y + 0);
            //_dragPopup.InvalidateArrange();
        }
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
        _dragPopup?.Close();
        //_dragPopup.Children.Clear();
        _dragPopup = null;
        if (sender is not DataGrid dataGrid) return;
        var item = e.DataTransfer.Items.FirstOrDefault(i => i.Formats.Contains(RowDragFormat));
        var droppedData = item?.TryGetRaw(RowDragFormat);
        if (droppedData == null) return;

        // 2. Wyznaczenie miejsca docelowego
        var visual = e.Source as Visual;
        var targetDataGrid = visual?.FindAncestorOfType<DataGrid>();
        var targetRowTarget = visual?.FindAncestorOfType<DataGridRow>();
        if (targetRowTarget == null) return;
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

    #endregion
}
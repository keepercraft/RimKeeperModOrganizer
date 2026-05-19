using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
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
        DataContext = viewModel;

        InitializeComponent();
        //AvaloniaXamlLoader.Load(this);

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

            ModsGrid?.LockColumnsWidth = (Content == null);
            ModsGridConfig?.LockColumnsWidth = (Content == null);
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
    private DataGridRow? _drag_Popup_Row;
    private DataGrid? _drag_Popup_DataGrid;
    private bool? _drag_Popup_Row_Offset;
    private IList? _drag_Popup_list;
    private double _drag_Popup_Height;
    private Popup? _drag_Popup;
    private double _drag_Popup_Start_Position_Delta = 10;
    private Point? _drag_Popup_Start_Position;
    private async void DataGrid_PointerPressedSelection(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not DataGrid context) return;
        _drag_Popup_Start_Position = e.GetPosition(this);
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
            //Debug.WriteLine("GetPosition X:" + _drag_Popup.HorizontalOffset + " Y:" + _drag_Popup.VerticalOffset + " - "+ tt?.Name??"?");
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
            Drag_Row_Finish(sender, e, drag_source);
        }
        Drag_Row_Highlight_Remove(drag_source);
        Drag_Popup_Close();
        _drag_Popup_Start_Position = null;
        _drag_Popup_DataGrid = null;
        _drag_Popup_Row = null;
        _drag_Popup_Row_Offset = null;
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

    public void Drag_Row_Finish(object? sender, PointerEventArgs e, DataGrid drag_source)
    {
        DataGrid? drag_target = _drag_Popup_DataGrid ?? _drag_Popup_Row?.FindAncestorOfType<DataGrid>();
        if (drag_target == null) return;

        //  var itemsSource = _drag_Popup_Start_Source.ItemsSource;
        //  var itemsTarget = drag_target.ItemsSource;
        var itemsSourceObservable = GetObservableSorce<ModModel>(drag_source);
        var itemsTargetObservable = GetObservableSorce<ModModel>(drag_target);
        //if (itemsSourceObservable != itemsTargetObservable) return;
        if (itemsSourceObservable == null || itemsTargetObservable == null || _drag_Popup_list == null) return;

        ModModel? itemTarget = _drag_Popup_Row?.DataContext as ModModel ?? itemsTargetObservable?.LastOrDefault();
        if (itemTarget == null) return;

        int p_index_target = itemsTargetObservable.IndexOf(itemTarget);
        var p_index_source = _drag_Popup_list.Cast<ModModel>().Select(s => itemsSourceObservable.IndexOf(s));
        if (itemTarget != itemsTargetObservable?.LastOrDefault() && p_index_source.Contains(p_index_target)) return;
        bool reverse = p_index_source.Last() > p_index_target;
        var data_list = reverse
            ? _drag_Popup_list.Cast<ModModel>().Reverse()
            : _drag_Popup_list.Cast<ModModel>();

        Debug.WriteLine($"REVERSE:{reverse} = {p_index_source.Last()}>{p_index_target}");

        bool offset = _drag_Popup_Row_Offset ?? false;

        int? imove = null;
        ModModel? itemTarget_next = itemTarget;
        int ii = 0;
        foreach (var item in _drag_Popup_list.Cast<ModModel>())
        {
            int itemIndexSource = itemsSourceObservable?.IndexOf(item) ?? 0;
            int itemIndexTarget = itemsTargetObservable.IndexOf(itemTarget_next);
            int index = itemIndexSource < itemIndexTarget ? itemIndexTarget - 1 : itemIndexTarget;
            //if (imove == 0) 
            if (!offset || ii>0) index++;
            //index++;
            if (index > itemsTargetObservable.Count-1) 
                index = itemsTargetObservable.Count-1;
            item.Position = drag_target == ModsGridConfig ? index : null;
            if (itemIndexSource != index)
                itemsTargetObservable.Move(itemIndexSource, index);
            Debug.WriteLine($"MOVE:{itemIndexSource}->{itemIndexTarget}+{offset} {item.Label}->{itemTarget_next.Label}");
            imove = index;
            itemTarget_next = item;
            ii++;
        }
        if (drag_target == ModsGridConfig || drag_source == ModsGridConfig)
        {
            var list = GetObservableSorce<ModModel>(ModsGridConfig);
            int i = 0;
            foreach (var item in list.Where(w => w.Position != null))
            {
                item.Position = i++;
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
    public bool Drag_Row_Highlight(object? sender, PointerEventArgs e, DataGrid context)
    {
        var pointInGrid = e.GetPosition(context);
        var visualUnderCursor = context.InputHitTest(pointInGrid);
        if (visualUnderCursor is Control contextSource)
        {
            var row = contextSource.FindAncestorOfType<DataGridRow>();
            _drag_Popup_DataGrid = contextSource.FindAncestorOfType<DataGrid>();
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
            else
            {
                _drag_Popup_Row = null;
                _drag_Popup_Row_Offset = null;
            }
        }
        else
        {
            _rowSeparator.IsVisible = false;
        }
        return false;
    }
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

        var color = Application.Current!.RequestedThemeVariant == ThemeVariant.Light
            ? "#EEEEEE"
            : "#111111";
        var background = new SolidColorBrush(Avalonia.Media.Color.Parse(color));
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
                Background = background,
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
}
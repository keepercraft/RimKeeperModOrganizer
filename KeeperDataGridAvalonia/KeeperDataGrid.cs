using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.VisualTree;
using KeeperDataGridAvalonia.Extensions;
using KeeperDataGridAvalonia.Models;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
namespace KeeperDataGridAvalonia;

public partial class AdvancedFilterDataGridStyles : Styles { }
public class KeeperDataGrid : DataGrid
{
    public static readonly RoutedEvent<PointerPressedEventArgs> PointerPressedSelectionEvent =
        RoutedEvent.Register<InputElement, PointerPressedEventArgs>(
            nameof(PointerPressedSelection),
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
    public event EventHandler<PointerPressedEventArgs>? PointerPressedSelection
    {
        add { AddHandler(PointerPressedSelectionEvent, value); }
        remove { RemoveHandler(PointerPressedSelectionEvent, value); }
    }

    protected override Type StyleKeyOverride => typeof(DataGrid);
    public KeeperDataGrid() : base()
    {
        this.Styles.Add(new AdvancedFilterDataGridStyles());
        AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        AddHandler(PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
        //AddHandler(PointerPressedSelectionEvent, OnPointerReleased2, RoutingStrategies.Tunnel);
        ColumnsConfigProperty.Changed.AddClassHandler<KeeperDataGrid>((x, e) => x.OnColumnsConfigChanged(e));
        Columns.CollectionChanged += Columns_CollectionChanged;
    }

    //private void OnPointerReleased2(object? sender, PointerPressedEventArgs e)
    //{
    //    Debug.WriteLine("KeeperDataGrid PointerPressedSelectionEvent OK");
    //}

    private void Columns_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var item in e.NewItems)
            if (item is AdvancedFilterDataGrid col)
                col.ColumnBinding(ColumnsConfig);    
    }

    private bool _isHandlingPointerSelection = false;
    private PointerPressedEventArgs? _flagPinterSelection = null;
    private void RisePointerPressedSelectionEvent()
    {
        if (_flagPinterSelection == null) return;
        _flagPinterSelection.RoutedEvent = PointerPressedSelectionEvent;
        RaiseEvent(_flagPinterSelection);
        _flagPinterSelection = null;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not KeeperDataGrid context) return;
        if (e.Source is not Visual contextSource) return;
        if (contextSource.FindAncestorOfType<DataGridRow>() is not DataGridRow row) return;

        _flagPinterSelection = e;
        var contextRow = row?.DataContext;
        if (contextRow != null && !context.SelectedItems.Contains(contextRow))
        {
            e.Handled = false;
            _isHandlingPointerSelection = false;
        }
        else
        {
            RisePointerPressedSelectionEvent();
            e.Handled = true;
            _isHandlingPointerSelection = true;

        }

        //var header = (e.Source as Visual)?.FindAncestorOfType<DataGridColumnHeader>();
        //Debug.WriteLine("KeeperDataGrid Pressed");
    }
    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is not KeeperDataGrid context) return;
        if (e.Source is not Control contextSource) return;
        if (contextSource.FindAncestorOfType<DataGridRow>() is not DataGridRow row) return;

        bool overGrid = this.IsPointerOver;
        if (row != null && row.IsPointerOver)
        {
        }
        else
        {
            e.Handled = true;
            return;
        }

        var contextRow = contextSource.FindAncestorOfType<DataGridRow>()?.DataContext;
        if (contextRow != null)
        {
            var selectedSame = context.SelectedItems.Contains(contextRow);

            context.SelectedItem = contextRow;

            if (!selectedSame)
            {
                var properties = new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.Other);
                _flagPinterSelection = new PointerPressedEventArgs(
                    this,                          // Source: Twoja kontrolka
                    null!,                         // IPointer: może być null przy dummy, jeśli logika go nie używa
                    this,                          // Visual: punkt odniesienia dla współrzędnych
                    new Point(0, 0),               // Współrzędne (puste)
                    0,                             // Timestamp
                    properties,                    // Właściwości przycisku
                    KeyModifiers.None              // Modyfikatory klawiszy
                )
                {
                    RoutedEvent = PointerPressedSelectionEvent // Przypisujemy ID Twojego zdarzenia
                };
                RisePointerPressedSelectionEvent();
            }
        }
        //e.Handled = true;
        Debug.WriteLine("KeeperDataGrid Released");
    }
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        RaiseEvent(e);
        if (_flagPinterSelection != null)
        {
            Debug.WriteLine("KeeperDataGrid Pinter-Selection");
            RisePointerPressedSelectionEvent();
        }
    }

    #region ColumnsConfig
    public static readonly StyledProperty<ObservableCollection<IColumnConfig>?> ColumnsConfigProperty =
        AvaloniaProperty.Register<AdvancedFilterDataGrid, ObservableCollection<IColumnConfig>?>(nameof(ColumnsConfig),defaultValue: null);
    public ObservableCollection<IColumnConfig>? ColumnsConfig
    {
        get => GetValue(ColumnsConfigProperty);
        set => SetValue(ColumnsConfigProperty, value);
    }
    private void OnColumnsConfigChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is INotifyCollectionChanged oldCollection) oldCollection.CollectionChanged -= OnColumnsCollectionChanged;
        if (e.NewValue is INotifyCollectionChanged newCollection) newCollection.CollectionChanged += OnColumnsCollectionChanged;
        RebuildColumns(this);
    }
    private void OnColumnsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
       RebuildColumns(this);
    }
    public void RebuildColumns(KeeperDataGrid grid)
    {
        var cols = grid.Columns.Where(w => w is AdvancedFilterDataGrid).ToList();
        foreach (var item in cols)
            if (item is AdvancedFilterDataGrid col)
                col.ColumnBinding(ColumnsConfig);
    }
    #endregion

    protected override void OnColumnReordered(DataGridColumnEventArgs e)
    {
        int index = 0;
        foreach (var column in Columns.OrderBy(c => c.DisplayIndex))
        {
            if (column is AdvancedFilterDataGrid col) 
                col.ColumnIndex = index;
            column.DisplayIndex = index;
            index++;
        }
    }

    protected override void OnAutoGeneratingColumn(DataGridAutoGeneratingColumnEventArgs e)
    {
        base.OnAutoGeneratingColumn(e);
    }

    private void DataGrid_OnColumnReordered(object? sender, DataGridColumnEventArgs e)
    {

    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        //Debug.WriteLine("DataGrid: " + change.Property.Name);
        base.OnPropertyChanged(change);
        if (change.Property == ItemsSourceProperty)
        {
            var oldValues = change.OldValue as IEnumerable;
            var newValues = change.NewValue as IEnumerable;
            if (change.OldValue is IDataGridCollectionView oldView) oldView.CollectionChanged -= View_CollectionChanged;
            if (change.NewValue is IDataGridCollectionView newView) newView.CollectionChanged += View_CollectionChanged;
            if (newValues is IDataGridCollectionView view)
            {
                view.CombineFilters(FilterableTextColumnFilter);
            }
        }
    }
    private void View_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            if (sender is IDataGridCollectionView view)
            {
                SyncSortArrows(view.SortDescriptions);
            }
        }
    }
    private void SyncSortArrows(DataGridSortDescriptionCollection sortDescriptions)
    {
        //var sortDescription = DataGridSortDescription.FromPath("Name", ListSortDirection.Ascending);
        foreach (var sort in sortDescriptions)
        {
        }
        foreach (var col in this.Columns)
        {
            //col.SortDirection = null;           
        }
    }

    private bool FilterableTextColumnFilter(object obj)
    {
        if (obj == null || base.Columns == null) return true;
        foreach (var col in base.Columns.OfType<AdvancedFilterDataGrid>())
        {
            if (string.IsNullOrEmpty(col.FilterValue)) continue;
            var value = col.GetRowValue(obj)?.ToString();
            if (value == null || !value.Contains(col.FilterValue, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }
        return true;
    }
}
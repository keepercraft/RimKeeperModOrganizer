using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.VisualTree;
using KeeperDataGridAvalonia.Extensions;
using KeeperDataGridAvalonia.Models;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
namespace KeeperDataGridAvalonia;

public partial class AdvancedFilterDataGridStyles : Styles { }
public class KeeperDataGrid : DataGrid
{
    protected override Type StyleKeyOverride => typeof(DataGrid);
    public KeeperDataGrid() : base()
    {
        this.Styles.Add(new AdvancedFilterDataGridStyles());
        AddHandler(PointerPressedEvent, OnPointerPressed, handledEventsToo: true);
        ColumnsConfigProperty.Changed.AddClassHandler<KeeperDataGrid>((x, e) => x.OnColumnsConfigChanged(e));
        Columns.CollectionChanged += Columns_CollectionChanged;
    }

    private void Columns_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var item in e.NewItems)
            if (item is AdvancedFilterDataGrid col)
                col.ColumnBinding(ColumnsConfig);    
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var header = (e.Source as Visual)?.FindAncestorOfType<DataGridColumnHeader>();
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
        foreach (var column in Columns)
            if (column is AdvancedFilterDataGrid col)
                col.ColumnIndex = col.DisplayIndex;
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
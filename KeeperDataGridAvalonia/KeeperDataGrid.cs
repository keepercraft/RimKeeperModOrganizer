using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.VisualTree;
using KeeperDataGridAvalonia.Extensions;
using KeeperDataGridAvalonia.Models;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
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
        {
            if(item is AdvancedFilterDataGrid col)
                foreach (var conf in ColumnsConfig)
                {
                    RebuildColumnBinding(conf, col);
                }
        }      
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var header = (e.Source as Visual)?.FindAncestorOfType<DataGridColumnHeader>();
    }

    #region ColumnsConfig
    public static readonly StyledProperty<ObservableCollection<ColumnConfig>?> ColumnsConfigProperty =
        AvaloniaProperty.Register<AdvancedFilterDataGrid, ObservableCollection<ColumnConfig>?>(nameof(ColumnsConfig),defaultValue: null);
    public ObservableCollection<ColumnConfig>? ColumnsConfig
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
    }
    public void RebuildColumnBinding(ColumnConfig config, AdvancedFilterDataGrid column)
    {
        if (config.PropertyName != column.Key) return;

        //column.Binding = new Binding(config.PropertyName);
        column.Bind(DataGridColumn.HeaderProperty, new Binding
        {
            Source = config,
            Path = nameof(config.Header),
            Mode = BindingMode.TwoWay
        });
        column.Bind(AdvancedFilterDataGrid.WidthProperty, new Binding
        {
            Source = config,
            Path = nameof(config.Width),
            Mode = BindingMode.TwoWay
        });
        column.Bind(AdvancedFilterDataGrid.ShowFilterProperty, new Binding
        {
            Source = config,
            Path = nameof(config.ShowFilter),
            Mode = BindingMode.TwoWay
        });
        column.Bind(AdvancedFilterDataGrid.IsVisibleProperty, new Binding
        {
            Source = config,
            Path = nameof(config.IsVisible),
            Mode = BindingMode.TwoWay
        });
        column.Bind(AdvancedFilterDataGrid.FilterValueProperty, new Binding
        {
            Source = config,
            Path = nameof(config.Filter),
            Mode = BindingMode.TwoWay
        });
        column.Bind(AdvancedFilterDataGrid.ColumnIndexProperty, new Binding
        {
            Source = config,
            Path = nameof(config.ColumnIndex),
            Mode = BindingMode.TwoWay
        });

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
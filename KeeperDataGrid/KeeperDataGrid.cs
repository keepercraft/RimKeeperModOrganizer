using KeeperDataGrid.Extensions;
using KeeperDataGrid.Models;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
namespace KeeperDataGrid;

public class AdvancedFilterDataGrid : DataGrid
{
   // public List<ColumnState> ColumnsState { get; } = new();
    static AdvancedFilterDataGrid()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AdvancedFilterDataGrid),
            new FrameworkPropertyMetadata(typeof(AdvancedFilterDataGrid))
        );
    }

    public AdvancedFilterDataGrid()
    {
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        InitializeCollectionView();
    }

    private void InitializeCollectionView()
    {
        if (ItemsSource == null) return; 
        var view = System.Windows.Data.CollectionViewSource.GetDefaultView(ItemsSource);
    }


    protected override void OnAutoGeneratingColumn(DataGridAutoGeneratingColumnEventArgs e)
    {
        base.OnAutoGeneratingColumn(e);
        // 1. Sprawdzamy, czy generowana kolumna to tekst (standardowo DataGridTextColumn)
        //if (e.PropertyType == typeof(string))
        //{
        // 2. Tworzymy Twoją własną kolumnę
        //}
        if (Columns.Cast<FilterableTextColumn>().Any(c => c.Key == e.PropertyName))
        {
            e.Cancel = true;
        }
        else
        {
            var customColumn = new FilterableTextColumn
            {
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                Key = e.PropertyName,
                Header = e.PropertyName, // Przepisujemy nazwę właściwości jako nagłówek
                Binding = new Binding(e.PropertyName) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
                HeaderStyle = this.ColumnHeaderStyle
            };
            e.Column = customColumn;

            if (ColumnsConfig != null)
            {
                var config = ColumnsConfig.FirstOrDefault(c => c.PropertyName == e.PropertyName);
                if (config == null)
                {
                    config = new ColumnConfig
                    {
                        PropertyName = e.PropertyName,
                        Header = e.Column.Header.ToString(),
                        Width = e.Column.Width,
                    };
                    ColumnsConfig.Add(config);
                }
                config.SetBinding(e.Column);
            }
        }
    }

    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        base.OnItemsSourceChanged(oldValue, newValue);
        var view = CollectionViewSource.GetDefaultView(newValue);
        view.CombineFilters(FilterableTextColumnFilter);

        if (oldValue is ICollectionView oldView) oldView.CollectionChanged -= View_CollectionChanged;
        if (newValue is ICollectionView newView) newView.CollectionChanged += View_CollectionChanged;
        //if (newValue is ICollectionView view2) SyncSortArrows(view2.SortDescriptions);
    }
    private bool FilterableTextColumnFilter(object obj)
    {
        if (obj == null) return false;
        if (base.Columns == null) return true;
        foreach (var col in base.Columns.OfType<FilterableTextColumn>())
        {
            if (string.IsNullOrEmpty(col.FilterValue)) continue;
            var propertyName = (col.Binding as Binding)?.Path.Path;
            if (string.IsNullOrEmpty(propertyName)) continue;
            var propInfo = obj.GetType().GetProperty(propertyName);
            var value = propInfo?.GetValue(obj)?.ToString();
            if (value == null || !value.Contains(col.FilterValue, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }
        return true;
    }

    private void View_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            if (sender is ICollectionView view)
            {
                SyncSortArrows(view.SortDescriptions);
            }
        }
    }
    private void SyncSortArrows(SortDescriptionCollection sortDescriptions)
    {
        foreach (var col in this.Columns)
        {
            col.SortDirection = null;
        }
        if (sortDescriptions == null || sortDescriptions.Count == 0) return;
        foreach (SortDescription sd in sortDescriptions)
        {
            var column = this.Columns.FirstOrDefault(c =>
                c.SortMemberPath == sd.PropertyName ||
                (c is DataGridBoundColumn bc && (bc.Binding as Binding)?.Path.Path == sd.PropertyName));

            if (column != null)
            {
                column.SortDirection = sd.Direction;
            }
        }
    }

    protected override void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseRightButtonUp(e);
        if (IsInTextBox(e.OriginalSource as DependencyObject)) return;
        var dep = e.OriginalSource as DependencyObject;
        while (dep != null && dep is not DataGridColumnHeader) dep = VisualTreeHelper.GetParent(dep);
        if (dep is DataGridColumnHeader header)
            if (this.ItemsSource is ICollectionView view)
                view.SortDescriptions.Clear();
    }
    private bool IsInTextBox(DependencyObject? obj)
    {
        while (obj != null)
        {
            if (obj is TextBox) return true;
            obj = VisualTreeHelper.GetParent(obj);
        }
        return false;
    }

    #region ColumnsConfig
    public ObservableCollection<ColumnConfig> ColumnsConfig
    {
        get => (ObservableCollection<ColumnConfig>)GetValue(ColumnsConfigProperty);
        set => SetValue(ColumnsConfigProperty, value);
    }
    public static readonly DependencyProperty ColumnsConfigProperty =
            DependencyProperty.Register(nameof(ColumnsConfig),
                typeof(ObservableCollection<ColumnConfig>),
                typeof(AdvancedFilterDataGrid),
                new PropertyMetadata(null, OnColumnsConfigChanged));

    private static void OnColumnsConfigChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var grid = d as AdvancedFilterDataGrid;
        if (grid == null || e.NewValue == null) return;
        var configList = e.NewValue as ObservableCollection<ColumnConfig>;
        if (configList == null) return;

        grid.Columns.Clear();
        foreach (var config in configList)
        {
            var customColumn = new FilterableTextColumn
            {
                Key = config.PropertyName,
                Width = config.Width,
                Header = config.Header,
                Binding = new Binding(config.PropertyName) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
            };
            grid.Columns.Add(customColumn);
            config.SetBinding(customColumn);
        }
    }
    #endregion

}
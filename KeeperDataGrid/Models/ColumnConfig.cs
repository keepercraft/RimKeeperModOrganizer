using KeeperDataGrid.Extensions;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
namespace KeeperDataGrid.Models;

public class ColumnConfig : PropertyModel
{
    public string PropertyName { get; set; } = string.Empty;

    private string? _header;
    public string? Header
    {
        get => _header;
        set { _header = value; OnPropertyChanged(); }
    }

    private DataGridLength _width = new DataGridLength(1, DataGridLengthUnitType.Star);
    public DataGridLength Width
    {
        get => _width;
        set { _width = value; OnPropertyChanged(); }
    }

    private bool _isVisible = true;
    public bool IsVisible
    {
        get => _isVisible;
        set { _isVisible = value; OnPropertyChanged(); }
    }

    private string? _filter;
    public string? Filter
    {
        get => _filter;
        set { _filter = value; OnPropertyChanged(); }
    }

    private bool _showFilter = true;
    public bool ShowFilter
    {
        get => _showFilter;
        set { _showFilter = value; OnPropertyChanged(); }
    }

    private int? _columnIndex;
    public int? ColumnIndex
    {
        get => _columnIndex;
        set { _columnIndex = value; OnPropertyChanged(); }
    }

    public static ColumnConfig Create<T>(
        Expression<Func<T, object?>> expr,
        string? header = null,
        DataGridLength? width = null,
        int? columnIndex = null,
        bool isVisible = true)
    {
        string key = expr.GetPropertyName();
        return new ColumnConfig
        {
            PropertyName = key,
            Header = header??key,
            Width = width??new DataGridLength(1, DataGridLengthUnitType.Star),
            ColumnIndex = columnIndex,
            IsVisible = isVisible
        };
    }
}

public static class ColumnConfigExtension
{
    public static void RebuildColumns(this AdvancedFilterDataGrid grid, IEnumerable<ColumnConfig>? configs = null)
    {
        var data_config = configs ?? grid.ColumnsConfig;
        if (data_config == null) return;
        grid.Columns.Clear();
        foreach (var config in data_config)
        {
            grid.Columns.Add(config.MakeColumn());
        }
        grid.Columns.SyncColumnsDisplayIndex();
    }

    public static FilterableTextColumn MakeColumn(this ColumnConfig config, string? key = null, Style? style =  null)
    {
        var model = new FilterableTextColumn(key ?? config.PropertyName, style);
        config.SetBinding(model);
        return model;
    }

    public static void SetBinding(this ColumnConfig config, DataGridColumn column)
    {
        Binding headerBinding = new Binding(nameof(ColumnConfig.Header))
        {
            Source = config,
            Mode = BindingMode.TwoWay,
        };
        BindingOperations.SetBinding(column, DataGridColumn.HeaderProperty, headerBinding);

        Binding widthBinding = new Binding(nameof(ColumnConfig.Width))
        {
            Source = config,
            Mode = BindingMode.TwoWay,
        };
        BindingOperations.SetBinding(column, DataGridColumn.WidthProperty, widthBinding);

        Binding visBinding = new Binding(nameof(ColumnConfig.IsVisible))
        {
            Source = config,
            Mode = BindingMode.TwoWay,
            Converter = new BooleanToVisibilityConverter()
        };
        BindingOperations.SetBinding(column, DataGridColumn.VisibilityProperty, visBinding);

        Binding evisBinding = new Binding(nameof(ColumnConfig.ShowFilter))
        {
            Source = config,
            Mode = BindingMode.TwoWay,
            Converter = new BooleanToVisibilityConverter()
        };
        BindingOperations.SetBinding(column, FilterableTextColumn.ShowFilterProperty, evisBinding);

        Binding filterBinding = new Binding(nameof(ColumnConfig.Filter))
        {
            Source = config,
            Mode = BindingMode.TwoWay,
        };
        BindingOperations.SetBinding(column, FilterableTextColumn.FilterValueProperty, filterBinding);

        Binding coliBinding = new Binding(nameof(ColumnConfig.ColumnIndex))
        {
            Source = config,
            Mode = BindingMode.TwoWay,
        };
        BindingOperations.SetBinding(column, FilterableTextColumn.ColumnIndexProperty, coliBinding);
    }
}
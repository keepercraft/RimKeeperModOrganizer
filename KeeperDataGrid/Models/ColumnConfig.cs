using System.Windows.Controls;
using System.Windows.Data;
namespace KeeperDataGrid.Models;

public class ColumnConfig : PropertyModel
{

    public string PropertyName { get; set; } = string.Empty;
    public string? Header { get; set; }
    
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
}

public static class ColumnConfigExtension
{
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
    }
}
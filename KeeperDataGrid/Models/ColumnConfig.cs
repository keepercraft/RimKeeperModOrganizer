using KeeperDataGrid.Extensions;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
namespace KeeperDataGrid.Models;

public class ColumnConfig : KeeperPropertyModel
{
    public string PropertyName { get; set; } = string.Empty;

    private string? _header;
    public string? Header
    {
        get => _header;
        set { _header = value; OnPropertyChanged(); }
    }

    //private DataGridLength _width = new DataGridLength(1, DataGridLengthUnitType.Star);
    //public DataGridLength Width
    //{
    //    get => _width;
    //    set { _width = value; OnPropertyChanged(); }
    //}
    private string? _width;
    public string? Width
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
}

public static class ColumnConfigExtension
{
    public static void RebuildColumns(this AdvancedFilterDataGrid grid, IEnumerable<ColumnConfig>? configs = null)
    {
        var columns = grid.Columns.Where(w => w is FilterableTextColumn).Cast<FilterableTextColumn>().ToList();
        var data_config = configs ?? grid.ColumnsConfig;
        if (data_config == null) return;
        //grid.Columns.Clear();
        foreach (var config in data_config) //ADD-EDIT
        {
            var col = columns.FirstOrDefault(f => f.Key == config.PropertyName);
            if (col == null)
            {
                col = config.MakeColumn();
                grid.Columns.Add(col);
            }
            config.SetBinding(col);
        }
        //if(!grid.AutoGenerateColumns)
        //    foreach (var col in columns) //REMOVE
        //    {
        //        if(!data_config.Any(f => f.PropertyName == col.Key))
        //        {
        //            grid.Columns.Remove(col);
        //        }
        //    }
        grid.Columns.SyncColumnsDisplayIndex();
    }

    public static FilterableTextColumn MakeColumn(this ColumnConfig config, string? key = null, Style? style =  null)
    {
        var model = new FilterableTextColumn(key ?? config.PropertyName, style);
        //config.SetBinding(model);
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

        //Binding widthBinding = new Binding(nameof(ColumnConfig.Width))
        //{
        //    Source = config,
        //    Mode = BindingMode.TwoWay,
        //    //Converter = new DataGridLengthToStringConverter(),
        //    //UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        //    //FallbackValue = new DataGridLength(1, DataGridLengthUnitType.Star),
        //    //TargetNullValue = new DataGridLength(1, DataGridLengthUnitType.Star),
        //};
        //BindingOperations.SetBinding(column, DataGridColumn.WidthProperty, widthBinding);
        //Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        //{
        //    Binding widthBinding = new Binding(nameof(ColumnConfig.Width))
        //    {
        //        Source = config,
        //        Mode = BindingMode.TwoWay,
        //        Converter = new DataGridLengthToStringConverter(),
        //        // Ważne: usuń UpdateSourceTrigger.PropertyChanged, jeśli nadal mrozi UI
        //        // Domyślny trigger dla Width jest bezpieczniejszy
        //    };
        //    BindingOperations.SetBinding(column, DataGridColumn.WidthProperty, widthBinding);
        //}), DispatcherPriority.Loaded);
        Binding widthBinding = new Binding(nameof(ColumnConfig.Width))
        {
            Source = config,
            Mode = BindingMode.TwoWay,
        };
        BindingOperations.SetBinding(column, FilterableTextColumn.WidthTextProperty, widthBinding);

        Binding visBinding = new Binding(nameof(ColumnConfig.IsVisible))
        {
            Source = config,
            Mode = BindingMode.TwoWay,
            Converter = new BooleanToVisibilityConverter(),
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
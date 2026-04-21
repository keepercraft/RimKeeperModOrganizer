using Avalonia.Controls;
using Avalonia.Data;
using KeeperDataGridAvalonia.Models;
namespace KeeperDataGridAvalonia.Extensions;

public static class ColumnExtension
{
    public static void ColumnBinding(this AdvancedFilterDataGrid column, IEnumerable<IColumnConfig>? configs)
    {
        if (configs == null) return;
        foreach (var config in configs)
            column.ColumnBinding(config);
    }
    public static void ColumnBinding(this AdvancedFilterDataGrid column, IColumnConfig? config)
    {
        if (config == null) return;
        if (config.PropertyName != column.Key) return;

        column.Bind(DataGridColumn.HeaderProperty, new Binding
        {
            Source = config,
            Path = nameof(config.Header),
            Mode = BindingMode.TwoWay
        });
        column.Bind(AdvancedFilterDataGrid.WidthTextProperty, new Binding
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
        column.Bind(DataGridColumn.IsVisibleProperty, new Binding
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
}

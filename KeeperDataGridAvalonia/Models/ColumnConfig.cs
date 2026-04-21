using KeeperBaseSharedLib.Models;
namespace KeeperDataGridAvalonia.Models;

public class ColumnConfig : PropertyModel
{
    public string PropertyName { get; set; } = string.Empty;

    private string? _header;
    public string? Header
    {
        get => _header;
        set { _header = value; OnPropertyChanged(); }
    }

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

    }
}
using KeeperBaseSharedLib.Models;

namespace KeeperDataGridAvalonia.Models;

public interface IColumnConfig
{
    string PropertyName { get; set; }
    string? Header { get; set; }
    string? Width { get; set; }
    bool IsVisible { get; set; }
    string? Filter { get; set; }
    bool ShowFilter { get; set; }
    int? ColumnIndex { get; set; }
}
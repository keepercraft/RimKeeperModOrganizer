using System.Collections.ObjectModel;
using System.Windows.Controls;
namespace KeeperDataGrid.Extensions;

public static class FilterableTextColumnExtension
{
    public static void SyncColumnsIndex(this ObservableCollection<DataGridColumn> Columns)
    {
        foreach (var col in Columns.Where(w=> w is FilterableTextColumn).Cast<FilterableTextColumn>())
        {
            col.ColumnIndex = col.DisplayIndex;
        }
    }
    public static void SyncColumnsDisplayIndex(this ObservableCollection<DataGridColumn> Columns)
    {
        int i = 0;
        foreach (var col in Columns.Where(w => w is FilterableTextColumn).Cast<FilterableTextColumn>().OrderBy(c => c.ColumnIndex))
        {
            col.DisplayIndex = i++;
        }
    }
}

using Avalonia.Controls;
namespace KeeperDataGridAvalonia.Extensions;

public static class DataGridExtensions
{
    public static void ToggleStarColumns(this DataGrid grid, bool? isStar = null)
    {
        bool hasStar = isStar ?? grid.Columns.Any(c => c.Width.IsStar);
        if (hasStar)
        {
            foreach (var column in grid.Columns)
            {
                column.Width = new DataGridLength(
                    column.ActualWidth,
                    DataGridLengthUnitType.Pixel);
            }
        }
        else
        {
            double total = grid.Columns.Sum(c => c.ActualWidth);

            foreach (var column in grid.Columns)
            {
                double ratio = column.ActualWidth / total;

                column.Width = new DataGridLength(
                    ratio,
                    DataGridLengthUnitType.Star);
            }
        }
    }
}
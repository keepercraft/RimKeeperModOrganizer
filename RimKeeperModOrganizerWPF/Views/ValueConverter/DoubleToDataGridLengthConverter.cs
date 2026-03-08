using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
namespace RimKeeperModOrganizerWPF.Views.ValueConverter;

public class DoubleToDataGridLengthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d && !double.IsNaN(d) && d >= 0)
            return new DataGridLength(d);
        return new DataGridLength(100); //new DataGridLength(1, DataGridLengthUnitType.Star); // domyślnie Star
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DataGridLength length)
        {
            // tylko Pixel ma sens do zwracania double
            if (length.UnitType == DataGridLengthUnitType.Pixel)
                return (double?)length.Value;

            // Auto lub Star → null w VM
            return 100;
        }
        return 100;
    }
}
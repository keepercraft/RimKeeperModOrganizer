using System.Globalization;
using System.Windows.Data;

namespace RimKeeperModOrganizerWPF.Views.ValueConverter;

public class BoolToSLRConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
            return b ? "S" : "L";
        return "R"; // null → R
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

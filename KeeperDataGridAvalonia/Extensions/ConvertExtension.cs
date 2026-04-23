using Avalonia.Controls;
namespace KeeperDataGridAvalonia.Extensions;

public static class ConvertExtension
{
    public static string ToString(this double length, string format) => length.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
    public static string ToXamlString(this DataGridLength length)
    {
        if (length.IsAuto) return "Auto";
        if (length.IsStar)
            return length.Value.ToString("0.###*");
        //     length.DesiredValue > 0 
        //  ? length.DisplayValue.ToString(System.Globalization.CultureInfo.InvariantCulture) 
        //  : 
        //$"{length.Value}*";
        return length.Value.ToString("0.###");
    }
}
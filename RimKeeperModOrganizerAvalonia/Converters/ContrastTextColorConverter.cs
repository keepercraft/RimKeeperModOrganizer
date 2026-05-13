using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Globalization;
namespace RimKeeperModOrganizerAvalonia.Converters;

public class ContrastTextColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            var theme = Application.Current.ActualThemeVariant;
            return theme == ThemeVariant.Light
                ? Brushes.Black
                : Brushes.White;
        }
        Color? color = null;
        if (value is Avalonia.Media.Immutable.ImmutableSolidColorBrush) color = ((Avalonia.Media.Immutable.ImmutableSolidColorBrush)value).Color;
        else if (value is Color) color = (Color)value;
        else if (value is string hex) color = Color.Parse(hex);

        var v1 = color?.ToHsv().V ?? 0;
        //var v2 = color.ToHsl();
        return v1 > 0.5
            ? Brushes.Black
            : Brushes.White;
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
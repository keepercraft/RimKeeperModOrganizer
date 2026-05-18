using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using RimKeeperModOrganizerAvalonia.Extensions;
using System;
using System.Globalization;
using System.IO;
namespace RimKeeperModOrganizerAvalonia.Converters;

public class PathToBitmapConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string? path = value as string;
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return ModIconConverter.Get("ImageIcon")?.Recolor(Avalonia.Media.Colors.Gray);
        try
        {
            return new Bitmap(path);
        }
        catch
        {
            return ModIconConverter.Get("WarningIcon")?.Recolor(Avalonia.Media.Colors.Gray);
        }     
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
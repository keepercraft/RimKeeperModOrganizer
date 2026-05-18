using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using RimKeeperModOrganizerAvalonia.Extensions;
using RimKeeperModOrganizerLib.Models;
using System;
using System.Globalization;
using System.IO;
namespace RimKeeperModOrganizerAvalonia.Converters;

public class ModIconConverter : IValueConverter
{
    public static DrawingImage? Get(string key, Color? color = null)
    {
        foreach (var d in Application.Current!.Resources.MergedDictionaries)
            if (d.TryGetResource(key, null, out var value))
                if (value is DrawingImage img)
                    return color.HasValue ? img.Recolor(color.Value) : img;
        return null;
    }

    public static WindowIcon CreateIconFromDrawingImage(DrawingImage? drawingImage, int size = 64)
    {
        var image = new Image
        {
            Source = drawingImage,
            Width = size,
            Height = size
        };
        image.Measure(new Size(size, size));
        image.Arrange(new Rect(0, 0, size, size));
        var rtb = new RenderTargetBitmap(new PixelSize(size, size));
        rtb.Render(image);
        using var ms = new MemoryStream();
        rtb.Save(ms);
        ms.Position = 0;
        return new WindowIcon(ms);
    }

    public static DrawingImage? GetIconKey(object? value)
    {
        if (value == null) return null;
        ModLocation location = ModLocation.Unknow;
        if (value is ModLocation lod)
        {
            location = lod;
        }
        else if (value is string txt)
        {
            if (string.IsNullOrEmpty(txt)) return Get("DashIcon");
            Enum.TryParse(txt, out location);
        }
        else if (value is ModModel mod)
        {
            location = mod.Location;
        }
        switch (location)
        {
            case ModLocation.Local:
                return Get("FolderIcon");
            case ModLocation.Steam:
                return Get("SteamIcon");
            case ModLocation.DLC:
                return Get("RimworldLogoIcon");
            case ModLocation.MetaData:
                return Get("MetaDataIcon");
            default:
                return Get("WarningIcon");
        }
    }

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return GetIconKey(value);
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => AvaloniaProperty.UnsetValue;
}

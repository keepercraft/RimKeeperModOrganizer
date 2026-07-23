using Avalonia.Media;
namespace RimKeeperModOrganizerAvalonia.Extensions;

public static class DrowingExtension
{
    public static Drawing RecolorDrawing(this Drawing drawing, Color color)
    {
        return drawing switch
        {
            GeometryDrawing gd => new GeometryDrawing
            {
                Geometry = gd.Geometry,
                Pen = gd.Pen,
                Brush = new SolidColorBrush(color) // <-- tylko kolor
            },

            DrawingGroup dg => Recolor(new DrawingImage { Drawing = dg }, color).Drawing!,

            _ => drawing
        };
    }
    public static DrawingImage? Recolor(this DrawingImage source, Color color)
    {
        if (source.Drawing is not DrawingGroup group)
            return source;

        var cloned = new DrawingGroup();

        foreach (var child in group.Children)
        {
            cloned.Children.Add(RecolorDrawing(child, color));
        }

        return new DrawingImage { Drawing = cloned };
    }

    public static string ToRGBString(this Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    public static string? NormalizeRGB(string? value) => Color.TryParse(value, out var color) ? color.ToRGBString() : null;
}
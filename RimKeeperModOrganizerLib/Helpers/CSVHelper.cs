using System.Text;
namespace RimKeeperModOrganizerLib.Helpers;

public static class CSVHelper
{
    private const char SEP = ';';

    public static void Export<T>(
        this IEnumerable<T> items,
        string filePath,
        Action<List<(string Header, Func<T, object?> Selector)>> config)
    {
        var columns = new List<(string Header, Func<T, object?> Selector)>();
        config(columns);

        using var writer = new StreamWriter(filePath, false, new UTF8Encoding(true));

        writer.WriteLine($"sep={SEP}");
        writer.WriteLine(string.Join(SEP, columns.Select(c => Escape(c.Header))));

        foreach (var item in items)
        {
            var values = columns
                .Select(c => Escape(c.Selector(item)?.ToString()));

            writer.WriteLine(string.Join(SEP, values));
        }
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.Contains('"'))
            value = value.Replace("\"", "\"\"");

        if (value.IndexOfAny([SEP, '"', '\n', '\r']) >= 0)
            value = $"\"{value}\"";

        return "\"" + value + "\"";
    }
}
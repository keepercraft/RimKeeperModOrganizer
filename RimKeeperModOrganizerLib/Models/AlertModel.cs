using KeeperBaseLib.Model;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
namespace RimKeeperModOrganizerLib.Models;

public sealed class AlertsModel : PropertyModel
{
    public ObservableCollection<AlertModel> Items { get; init; } = new();
    public bool HasAlert { get; private set; }
    public string Color { get; private set; }
    public AlertsModel()
    {
        Items.CollectionChanged += Items_CollectionChanged;
    }
    public void Clear(params AlertType[] types)
    {
        if (types == null || types.Length == 0)
        {
            Items.Clear();
            return;
        }
        var set = types.ToHashSet();
        for (int i = Items.Count - 1; i >= 0; i--)
            if (set.Contains(Items[i].Type))
                Items.RemoveAt(i);
    }
    public void Add(AlertLevel level, AlertType type, params object[] args) => Items.Add(new AlertModel { Level = level, Type = type, Args = args });
    private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        HasAlert = Items.Count > 0;
        Color = AlertRepository.GetColor(HasAlert ? Items.Max(a => a.Level) : null);
        OnPropertyChanged(nameof(HasAlert));
        OnPropertyChanged(nameof(Color));
    }
}

public sealed record class AlertModel
{
    public AlertLevel Level { get; init; }
    public AlertType Type { get; init; }
    public object[] Args { get; init; } = [];
    public string Message => AlertRepository.GetText(Type, "en", Args);
    public string Color => AlertRepository.GetColor(Level);
}

public enum AlertLevel : byte
{
    none = 0,
    Info = 1,
    Warning = 2,
    Critical = 3,
}

public enum AlertType
{
    None = 0,
    Missing,
    Version,
    Dependency,
    IncompatibleWith,
    LoadAfter,
    LoadBefore,
    DuplicatePackageID,
    MissingPackageID,
    MissingPath,
}

public static class AlertRepository
{
    public static string GetColor(AlertLevel? Level = null)
    {
        return Level switch
        {
            AlertLevel.Info => "#AAFFFF",
            AlertLevel.Warning => "#FFAA00",
            AlertLevel.Critical => "#FF0000",
            _ => "Transparent"
        };
    }

    public static string GetText(AlertType type, string culture = "en")
    {
        if (_texts.TryGetValue((type, culture), out var text)) return text;
        if (_texts.TryGetValue((type, "en"), out text)) return text;
        return type.ToString();
    }

    public static string GetText(AlertType type, string culture = "en", params object[] args)
    {
        if (!_texts.TryGetValue((type, culture), out var format)) return type.ToString();
        return string.Format(format, args);
    }

    private static readonly Dictionary<(AlertType, string), string> _texts = new()
    {
        [(AlertType.Missing, "en")] = "Missing: {0}",
        [(AlertType.MissingPath, "en")] = "Missing Path: {0}",
        [(AlertType.MissingPackageID, "en")] = "Missing PackageID: {0}",
        [(AlertType.DuplicatePackageID, "en")] = "Duplicate PackageID: {0}",
        [(AlertType.Version, "en")] = "Version Mismatch: {0}",
        [(AlertType.Dependency, "en")] = "Missing Dependency: {0}",
        [(AlertType.IncompatibleWith, "en")] = "Incompatible With: {0}",
        [(AlertType.LoadAfter, "en")] = "Load After: {0}",
        [(AlertType.LoadBefore, "en")] = "Load Before: {0}",
    };
}
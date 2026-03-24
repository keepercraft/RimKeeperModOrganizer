using KeeperDataGrid.Models;
using RimKeeperModOrganizerLib.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
namespace RimKeeperModOrganizerWPF.Extensions;

public static class ColumnConfigExtension
{
    public static void BindToSettings(this List<ColumnSettings> ModColumnData, ObservableCollection<ColumnConfig> ModsCollectionColumns)
    {
        foreach (var kvp in ModColumnData)
        {
            var config = new ColumnConfig
            {
                PropertyName = kvp.Key,
                Header = kvp.Name,
                Width = kvp.Width,
                IsVisible = kvp.Visible,
                ColumnIndex = kvp.DisplayIndex,
            };
            config.PropertyChanged += (s, e) => SyncConfig((ColumnConfig)s, e, ModColumnData);
            kvp.PropertyChanged += (s, e) => SyncConfig((ColumnSettings)s, e, ModsCollectionColumns);
            ModsCollectionColumns.Add(config);
        }
    }

    private static bool _updating = false;
    private static void SyncConfig(ColumnConfig config, PropertyChangedEventArgs e, List<ColumnSettings> target)
    {
        if (_updating) return;
        _updating = true;

        var settings = target.Find(c => c.Key == config.PropertyName);
        if (settings != null)
        {
            settings.Name = config.Header ?? config.PropertyName;
            settings.Width = config.Width ?? "*";
            settings.Visible = config.IsVisible;
            settings.DisplayIndex = config.ColumnIndex ?? 0;
        }
        _updating = false;
    }
    private static void SyncConfig(ColumnSettings config, PropertyChangedEventArgs e, ObservableCollection<ColumnConfig> target)
    {
        if (_updating) return;
        _updating = true;
        var settings = target.FirstOrDefault(c => c.PropertyName == config.Key);
        if (settings != null)
        {
            settings.IsVisible = config.Visible;
            settings.Width = config.Width;
            settings.ColumnIndex = config.DisplayIndex;
            settings.Header = config.Name;
        }
        _updating = false;
    }
}
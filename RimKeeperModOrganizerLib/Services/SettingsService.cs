using KeeperBaseLib.Helper.Reflection;
using RimKeeperModOrganizerLib.Helpers;
using RimKeeperModOrganizerLib.Models;
using System.Collections;
using System.Reflection;
using System.Text.Json;
namespace RimKeeperModOrganizerLib.Services;

public class SettingsService
{
    public string PathSettings { get; } = Path.Combine(AppContext.BaseDirectory, "AppSettings.json");
    public Dictionary<string,object> DataSettings { get; } = new() { {"SETTING", new SettingsModel()} };
    public SettingsModel Settings => (SettingsModel)DataSettings["SETTING"];
    public SettingsService()
    {
        StartLoad();
    }
    public void Load()
    {
        try
        {
            if (!File.Exists(PathSettings)) return;
            string json = File.ReadAllText(PathSettings);
            var doc = JsonDocument.Parse(json);
            foreach (var item in DataSettings)
            {
                if (doc.RootElement.TryGetProperty(item.Key, out var uiNode))
                {
                    var temp = JsonSerializer.Deserialize(uiNode, item.Value.GetType());
                    foreach (var prop in item.Value.GetType().GetProperties().Where(p => p.CanWrite))
                    {                      
                        var value = prop.GetValue(temp);
                        prop.SetValue(DataSettings[item.Key], value);
                    }
                }
            }
        }
        catch { }
    }
    public void Save()
    {
        try
        {
            string json = JsonSerializer.Serialize(DataSettings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(PathSettings, json);
        }
        catch { }
    }
    public void StartLoad()
    {
        bool fexist = File.Exists(PathSettings);
        if (fexist) Load(); 
        if(!fexist || string.IsNullOrEmpty(Settings.PathDirGame)) AutoFind();
        Save();
    }
    public void AutoFind()
    {
        if (string.IsNullOrEmpty(Settings.PathDirSteam))
            Settings.PathDirSteam = FileHelper.FindSteamInstallPath() ?? "";
        if (string.IsNullOrEmpty(Settings.PathDirGame))
            Settings.PathDirGame = FileHelper.FindRimWorldGamePath(Settings.PathDirSteam) ?? "";
        if (string.IsNullOrEmpty(Settings.PathDirModsSteam))
            Settings.PathDirModsSteam = FileHelper.FindRimWorldWorkshopModsPaths(Settings.PathDirSteam).FirstOrDefault() ?? "";
        if (string.IsNullOrEmpty(Settings.PathDirModsLocal))
            Settings.PathDirModsLocal = FileHelper.FindRimWorldLocalModsPath(Settings.PathDirGame) ?? "";
        if (string.IsNullOrEmpty(Settings.PathDirGameConfig))
            Settings.PathDirGameConfig = FileHelper.FindRimWorldConfigPath() ?? "";

        Settings.GameVersion = FileHelper.GetRimworldVersion(Settings.PathDirGame) ?? "0";
    }

    public void CreateCopy<T>(T copy) where T : class => CopyTo<T>(copy, GetValueOfType<T>(DataSettings));
    public void ApplyChanges<T>(T source) where T : class => CopyTo<T>(GetValueOfType<T>(DataSettings), source);
    private void CopyTo<T>(T? target, T? source) where T : class
    {
        if (target == null || source == null) return; 
        foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite))
        {
            if (typeof(IDictionary).IsAssignableFrom(prop.PropertyType))
            {
                if(prop.GetValue(source) is IDictionary cs && prop.GetValue(target) is IDictionary ct)
                {
                    foreach (var key in cs.Keys)
                    {
                        if (ct.Contains(key))
                        {
                            ct[key] = cs[key];
                        }
                    }
                }
            }
            else if (typeof(IList).IsAssignableFrom(prop.PropertyType))
            {
                if (prop.GetValue(source) is IList cs && prop.GetValue(target) is IList ct)
                {
                    for (int i = 0; i < cs.Count; i++)
                    {
                        if (i < ct.Count)
                        {
                            ct[i] = cs[i];
                        }
                    }
                }
            }
            else
            {
                var value = prop.GetValue(source);
                prop.SetValue(target, value);
            }
        }
    }
    public static T? GetValueOfType<T>(Dictionary<string, object> dict) where T : class
    {
        foreach (var kvp in dict)
        {
            if (kvp.Value is T value) return value;
        }
        return null;
    }
}

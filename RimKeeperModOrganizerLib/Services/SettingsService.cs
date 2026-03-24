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
            //JsonConvert.PopulateObject(File.ReadAllText(PathSettings), DataSettings);
            //var json = File.ReadAllText(PathSettings);
            //var jObject = JObject.Parse(json);
            //var settingNode = jObject["SETTING"];
            //JsonConvert.PopulateObject(settingNode.ToString(), Settings);

            using var fs = File.OpenRead(Settings.PathSettings);
            var doc = JsonDocument.Parse(fs);
            foreach (var item in DataSettings)
            {
                if (doc.RootElement.TryGetProperty(item.Key, out var uiNode))
                {
                    // Deserializuj JSON do tymczasowego obiektu tego samego typu
                    var temp = JsonSerializer.Deserialize(uiNode, item.Value.GetType());

                    if (temp != null)
                    {
                        // Aktualizuj wszystkie właściwości istniejącej instancji
                        MergeObjects(item.Value, temp);
                    }
                }
            }
        }
        catch 
        { 
        }
    }

    private void MergeObjects(object target, object source)
    {
        if (target == null || source == null) return;

        var type = target.GetType();

        foreach (var prop in type.GetProperties().Where(p => p.CanRead && p.CanWrite))
        {
            var sourceValue = prop.GetValue(source);
            var targetValue = prop.GetValue(target);

            if (sourceValue == null) continue;

            var propType = prop.PropertyType;

            // proste typy
            if (propType.IsPrimitive || propType.IsEnum || propType == typeof(string) || propType == typeof(decimal))
            {
                prop.SetValue(target, sourceValue);
            }
            // listy → aktualizacja elementów
            else if (typeof(System.Collections.IList).IsAssignableFrom(propType))
            {
                if (targetValue is System.Collections.IList targetList && sourceValue is System.Collections.IList sourceList)
                {
                    targetList.Clear();
                    foreach (var element in sourceList)
                        targetList.Add(element);
                }
                else
                {
                    prop.SetValue(target, sourceValue);
                }
            }
            // nested obiekty
            else
            {
                if (targetValue == null)
                {
                    prop.SetValue(target, sourceValue);
                }
                else
                {
                    MergeObjects(targetValue, sourceValue);
                }
            }
        }
    }

    public void Save() => JsonHelper.SerializeModel(DataSettings, PathSettings);
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

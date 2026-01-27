using RimKeeperModOrganizerLib.Helpers;
using RimKeeperModOrganizerLib.Models;
using RimKeeperModOrganizerWPF.Models;
using System.IO;
using System.Text.Json;

namespace RimKeeperModOrganizerWPF.Services;

public class SettingsService
{
    public SettingsModel Settings { get; } = new ();
    public SettingsService()
    {
        Load();
        AutoFind();
        Save();
    }
    public void Load()
    {
        try
        {
            Settings.PathSettings = Path.Combine(AppContext.BaseDirectory, Settings.PathSettings);
            Settings.PathModData = Path.Combine(AppContext.BaseDirectory, Settings.PathModData);

            if (!File.Exists(Settings.PathSettings)) return;

            string json = File.ReadAllText(Settings.PathSettings);
            var temp = JsonSerializer.Deserialize<SettingsModel>(json);
            if (temp == null) return;

            foreach (var prop in typeof(SettingsModel).GetProperties().Where(p => p.CanWrite))
            {
                var value = prop.GetValue(temp);
                prop.SetValue(Settings, value);
            }
        }
        catch { }
    }
    public void Save() 
    {
        try
        {
            string json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Settings.PathSettings, json);
        }
        catch { }
    }
    public void AutoFind()
    {
        if(string.IsNullOrEmpty(Settings.PathDirGame))
            Settings.PathDirGame = FileHelper.FindRimWorldGamePath() ?? "";
        if(string.IsNullOrEmpty(Settings.PathDirGameConfig)) 
            Settings.PathDirGameConfig = FileHelper.FindRimWorldConfigPath() ?? "";
        if(string.IsNullOrEmpty(Settings.PathDirModsSteam)) 
            Settings.PathDirModsSteam = FileHelper.FindRimWorldWorkshopModsPaths().FirstOrDefault() ?? "";
        if (string.IsNullOrEmpty(Settings.PathDirModsLocal))
            Settings.PathDirModsLocal = FileHelper.FindRimWorldLocalModsPath() ?? "";

        Settings.GameVersion = FileHelper.GetRimworldVersion(Settings.PathDirGame) ?? "0";
    }
    public void CreateCopy(SettingsModel copy) => CopyTo(copy, Settings);
    public void ApplyChanges(SettingsModel source) => CopyTo(Settings, source);
    private void CopyTo(SettingsModel target, SettingsModel source)
    {
        foreach (var prop in typeof(SettingsModel).GetProperties().Where(p => p.CanRead && p.CanWrite))
        {
            var value = prop.GetValue(source);
            prop.SetValue(target, value);
        }
    }

    public IEnumerable<ModModel> FindRimWorldAllMods()
    {
        foreach (ModModel item in FileHelper.GetMods(FileHelper.FindRimWorldDLCPath(Settings.PathDirGame), null))
        {
            yield return item;
        }
        foreach (ModModel item in FileHelper.GetMods(Settings.PathDirModsLocal, true))
        {
            yield return item;
        }
        foreach (ModModel item in FileHelper.FindRimWorldWorkshopModsPaths().SelectMany(s => FileHelper.GetMods(s, false)))
        {
            yield return item;
        }
    }
}
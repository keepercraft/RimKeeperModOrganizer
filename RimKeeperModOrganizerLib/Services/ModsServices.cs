using RimKeeperModOrganizerLib.Extensions;
using RimKeeperModOrganizerLib.Helpers;
using RimKeeperModOrganizerLib.Models;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace RimKeeperModOrganizerLib.Services;

public class ModsServices
{
 //   public ObservableCollection<ModModel> ModsList { get; } = new();
 //   public ObservableCollection<string> ModGroups { get; } = new();
 //   public ObservableCollection<string> ModColors { get; } = new();

    private readonly SettingsService _settingsService;
    public ModsServices(SettingsService SettingsService)
    {
        _settingsService = SettingsService;
    }

    public Action<bool> LoadModsActive { get; set; }
    private bool LoadModsFromLocalRunning = false;
    //public Task LoadModsFromLocalAsync(string? path = null) => Task.Run(LoadMods);
    public IEnumerable<ModModel> LoadMods(string? path = null)
    {
        if (LoadModsFromLocalRunning) yield break;
        LoadModsActive?.Invoke(LoadModsFromLocalRunning = true);

       // List<ModModel> modsToYield = new();
       // try
      // {
            string? aboutPath = path ?? FileHelper.GetModsConfig(_settingsService.Settings.PathDirGameConfig);
            if (aboutPath == null) yield break;

            ModsConfigModel? modsConfig = XMLHelper.LoadModsConfig(aboutPath);
            if (modsConfig == null) yield break;

            LocalDataListModel modsData = XMLHelper.LoadLocalData(_settingsService.Settings.PathModData) ?? new LocalDataListModel();

           // ModsList.Clear();
            foreach (ModModel mod in FindRimWorldAllMods())
            {
                mod.Position = modsConfig?.Position(mod.About?.PackageId) ?? -1;
                mod.TrySet(modsData);
                if (mod.Position >= 0) mod.Selected = true;
              //  ModsList.Add(mod);
                yield return mod;
            }
           //ModsList.SortCollectionByConfig(modsConfig);
           // ModListValidation(ModsList);
           /*
            foreach (var item in modsData.ModDataList.SelectMany(s => s.Groups).Where(w => !string.IsNullOrEmpty(w)).Distinct())
                if (!ModGroups.Contains(item))
                    ModGroups.Add(item);
            foreach (var item in modsData.ModDataList.Select(s => s.Color).Where(w => !string.IsNullOrEmpty(w)).Distinct())
                if (!ModColors.Contains(item))
                    ModColors.Add(item);
           */
       // }
       // catch (Exception ex)
       // {
       // }
       // finally
       // {
            LoadModsActive?.Invoke(LoadModsFromLocalRunning = false);
            GC.Collect();
       // }
    }
    public IEnumerable<ModModel> FindRimWorldAllMods()
    {
        foreach (ModModel item in FileHelper.GetMods(FileHelper.FindRimWorldDLCPath(_settingsService.Settings.PathDirGame), null))
        {
            yield return item;
        }
        foreach (ModModel item in FileHelper.GetMods(_settingsService.Settings.PathDirModsLocal, true))
        {
            yield return item;
        }
        foreach (ModModel item in FileHelper.FindRimWorldWorkshopModsPaths().SelectMany(s => FileHelper.GetMods(s, false)))
        {
            yield return item;
        }
    }
    public void SaveConfig(IEnumerable<ModModel> modlist, string? path = null)
    {
        var aboutPath = FileHelper.GetModsConfig(_settingsService.Settings.PathDirGameConfig);
        if (aboutPath == null) return;
        ModsConfigModel? mods = XMLHelper.LoadModsConfig(aboutPath);
        if (mods == null) return;
        mods.Version = _settingsService.Settings.GameVersion;
        mods.ActiveMods = modlist.Where(x => x.Data != null).Select(x => x.Data.PackageId).ToList();
        XMLHelper.SaveModsConfig(path??aboutPath, mods);
        
    }
    public void SaveLocalData(IEnumerable<ModModel> modlist)
    {
        LocalDataListModel? modsData = XMLHelper.LoadLocalData(_settingsService.Settings.PathModData) ?? new LocalDataListModel();
        modsData.ModDataList.Clear();
        foreach (var item in modlist.Where(x => x.Data != null).Where(x => x.Data.NotNull))
        {
            modsData.ModDataList.Add(item.Data);
        }
        XMLHelper.SaveLocalData(modsData, _settingsService.Settings.PathModData);
    }

    public Dictionary<string, string> LoadRimPyColors(string configPath)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!File.Exists(configPath))
            return result;

        bool inColors = false;

        foreach (var rawLine in File.ReadLines(configPath))
        {
            var line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith(";") || line.StartsWith("#"))
                continue;

            if (line.StartsWith("["))
            {
                inColors = line.Equals("[Colors]", StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (!inColors)
                continue;

            int eq = line.IndexOf('=');
            if (eq <= 0)
                continue;

            var key = line.Substring(0, eq).Trim();
            var value = line.Substring(eq + 1).Trim();

            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                result[key] = value;
        }

        return result;
    }

    public void ExportCSVMods(IEnumerable<ModModel> modlist, string path)
    {
        //var modconfig = _settingsService.Settings.ModColumnData.Where(w => w.Value.Visible).ToList();
        CSVHelper.Export(modlist, path, c =>
        {
           // c.Add(("*", m => m.Local));
            c.Add(("PackageId", m => m.About?.PackageId));
            c.Add(("SteamId", m => m.About?.SteamId));
            c.Add(("Name", m => m.Label));
            c.Add(("Path", m => m.Path));
            c.Add(("Autors", m => m.About?.Author));
            c.Add(("Versions", m => m.Versions));
            c.Add(("Color", m => m.Data?.Color));
            c.Add(("Groups", m => m.Data?.Group));
            //c.Add(("LoadAfter", m => m.About?.LoadAfter));
            //c.Add(("LoadBefore", m => m.About?.LoadBefore));
            //c.Add(("SupportedVersions", m => m.About?.SupportedVersions));
        });
    }
}

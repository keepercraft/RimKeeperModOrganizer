using RimKeeperModOrganizerLib.Extensions;
using RimKeeperModOrganizerLib.Helpers;
using RimKeeperModOrganizerLib.Models;
namespace RimKeeperModOrganizerLib.Services;

public class ModsServices
{
    private readonly SettingsService _settingsService;
    public ModsServices(SettingsService SettingsService)
    {
        _settingsService = SettingsService;

        if (!File.Exists(_settingsService.Settings.PathModData))
        {
            SaveLocalData();
        }
    }

    public Action<bool> LoadModsActive { get; set; }
    private bool LoadModsFromLocalRunning = false;
    public string? LastConfigLoad { get; set; }

    public IEnumerable<ModModel> LoadModMetaData(string? path = null)
    {
        if (LoadModsFromLocalRunning) yield break;
        LoadModsActive?.Invoke(LoadModsFromLocalRunning = true);

        ModsConfigModel? modsConfig = LoadModsConfig(path);
        //LocalDataListModel? modsData = LoadModData();
        LocalDataListModel2? modsData = LoadModData2();

        foreach (var packageId in modsConfig.ActiveMods)
        {
            var newMod = modsConfig.Make(packageId);
            //newMod.Data = modsData?.ModDataList.FirstOrDefault(x => x.GetKeyID() == packageId);
            newMod.Data = modsData?.ModDataList.FindByKey(packageId);//.FirstOrDefault(x => x.GetKeyID() == packageId).Value;
            yield return newMod;
        }
        if (modsData?.ModDataList != null)
        foreach (var item in modsData.ModDataList.Where(w => !modsConfig.ActiveMods.Contains(w.GetKeyID())))
        {
            yield return item.Make();
        }
        LoadModsActive?.Invoke(LoadModsFromLocalRunning = false);
    }

    public IEnumerable<ModModel> LoadModLazy()
    {
        foreach (var mod in FindRimWorldAllMods())
        {
            yield return mod;
        }
    }

    public IEnumerable<ModModel> RefreshMods(IEnumerable<ModModel> models) //to optimization
    {
        foreach (var model in models)
        {
            RefreshMod(model);
            yield return model;
        }
    }
    public ModModel RefreshMod(ModModel model)
    {
        if (!string.IsNullOrEmpty(model.Path))
        {
            model.Update(new ModModel(model.Path, ModLocation.Steam));
        }
        else
        {
            var newmod = FindRimWorldAllMods().FirstOrDefault(c => c.ModId == model.ModId);
            if (newmod != null) model.Update(newmod);
        }
        var newmoddata = LoadModData2()?.ModDataList.FindByKey(model.ModId);
        if (newmoddata != null) model.Update(newmoddata);
        model.RaisePropertyChanged();
        return model;
    }

    public LocalDataListModel2? LoadModData2(string? path = null)
    {
        return JsonHelper.DeserializeModel<LocalDataListModel2>(path ?? _settingsService.Settings.PathModData);
    }
    public LocalDataListModel? LoadModData(string? path = null)
    {
        return JsonHelper.DeserializeModel<LocalDataListModel>(path ?? _settingsService.Settings.PathModData);
    }
    public ModsConfigModel? LoadModsConfig(string? path = null)
    {
        string? aboutPath = LastConfigLoad = path ?? FileHelper.GetModsConfig(_settingsService.Settings.PathDirGameConfig);
        if (String.IsNullOrEmpty(aboutPath)) return null;
        return XMLHelper.LoadModsConfig(aboutPath);
    }
    public IEnumerable<ModModel> FindRimWorldAllMods()
    {
        foreach (ModModel item in FileHelper.GetMods(FileHelper.FindRimWorldDLCPath(_settingsService.Settings.PathDirGame), null, ModLocation.DLC))
        {
            yield return item;
        }
        foreach (ModModel item in FileHelper.GetMods(_settingsService.Settings.PathDirModsLocal, true, ModLocation.Local))
        {
            yield return item;
        }
        foreach (ModModel item in FileHelper.FindRimWorldWorkshopModsPaths().SelectMany(s => FileHelper.GetMods(s, false, ModLocation.Steam)))
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
        mods.ActiveMods = modlist
            .Where(x => x.Position != null)
            .OrderBy(x => x.Position)
            //.Select(x => x.About.PackageId)
            .Select(x => 
            {
                if(x.Location == ModLocation.Steam && modlist.Any(w => w.About?.PackageId == x.About.PackageId && w.Location == ModLocation.Local))
                {
                    return x.About.PackageId.ToSteamSuffix();
                }
                return x.About.PackageId;
            })
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct().ToList();
        XMLHelper.SaveModsConfig(path??aboutPath, mods);
        
    }
    public void SaveLocalData(IEnumerable<ModModel>? modlist = null, string? path = null)
    {
        string p = path ?? _settingsService.Settings.PathModData;
        if (string.IsNullOrEmpty(p)) return;
        LocalDataListModel2 modsData = new LocalDataListModel2();
        if (modlist != null)
            foreach (var item in modlist.Where(x => x.Data.IsNotNull()))
            {
                modsData.ModDataList.Add(item.ModId,item.Data);
            }
        JsonHelper.SerializeModel(modsData, p);
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
        CSVHelper.Export(modlist, path, c =>
        {
            c.Add(("Key", m => m.ModId));
            c.Add(("PackageId", m => m.About?.PackageId));
            c.Add(("SteamId", m => m.About?.SteamId));
            c.Add(("Name", m => m.Label));
            c.Add(("Path", m => m.Path));
            c.Add(("Autors", m => m.About?.Authors));
            c.Add(("Versions", m => m.Versions));
            c.Add(("Color", m => m.Data?.Color));
            c.Add(("Color", m => m.Data?.Comment));
            c.Add(("Groups", m => m.Data?.Group));
            c.Add(("Groups", m => m.Data?.PackageGroup));
        });
    }
}
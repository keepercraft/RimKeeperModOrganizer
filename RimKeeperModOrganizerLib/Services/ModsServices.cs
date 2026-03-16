using RimKeeperModOrganizerLib.Helpers;
using RimKeeperModOrganizerLib.Models;
using FileHelper = RimKeeperModOrganizerLib.Helpers.FileHelper;
namespace RimKeeperModOrganizerLib.Services;

public class ModsServices
{
    private readonly SettingsService _settingsService;
    public ModsServices(SettingsService SettingsService)
    {
        _settingsService = SettingsService;
    }

    public Action<bool> LoadModsActive { get; set; }
    private bool LoadModsFromLocalRunning = false;

    public IEnumerable<ModModel> LoadMods(string? path = null) //.old
    {
        if (LoadModsFromLocalRunning) yield break;
        LoadModsActive?.Invoke(LoadModsFromLocalRunning = true);

        ModsConfigModel? modsConfig = LoadModsConfig(path);
        if (modsConfig == null) yield break;

        LocalDataListModel modsData = JsonHelper.DeserializeModel<LocalDataListModel>(_settingsService.Settings.PathModData) ?? new LocalDataListModel();

        foreach (ModModel mod in FindRimWorldAllMods())
        {
            mod.Position = modsConfig?.Position(mod.About?.PackageId) ?? -1;
            mod.TrySet(modsData);
            if (mod.Position >= 0) mod.Selected = true;
            //  ModsList.Add(mod);
            yield return mod;
        }
        LoadModsActive?.Invoke(LoadModsFromLocalRunning = false);
        GC.Collect();
    }
    public IEnumerable<ModModel> LoadMods2(string? path = null)
    {
        if (LoadModsFromLocalRunning) yield break;
        LoadModsActive?.Invoke(LoadModsFromLocalRunning = true);

        List<ModModel> modList = FindRimWorldAllMods().ToList();

        ModsConfigModel? modsConfig = LoadModsConfig(path);
        if (modsConfig != null)
        {
            foreach (var item in modsConfig.ActiveMods)
            {
                var modListItem = modList.FirstOrDefault(x => x?.About?.PackageId == item);
                if (modListItem == null)
                {
                    modListItem = new ModModel() { About = new AboutModel() { PackageId = item } };
                    modList.Add(modListItem);
                }
                modListItem.Position = modsConfig?.Position(item) ?? -1;
                if (modListItem.Position >= 0) modListItem.Selected = true;
            }
        }

        LocalDataListModel? modsData = JsonHelper.DeserializeModel<LocalDataListModel>(_settingsService.Settings.PathModData);
        if (modsData != null)
        {
            foreach (var item in modsData.ModDataList)
            {
                var modListItem = modList.FirstOrDefault(x => x?.About?.PackageId == item.PackageId);
                if (modListItem == null)
                {
                    modListItem = new ModModel() { About = new AboutModel() { PackageId = item.PackageId } };
                    modList.Add(modListItem);
                }
                modListItem.Data = item;
            }
        }

        foreach (ModModel item in modList)
        {
            if(item.Data == null) item.Data = new ModDataModel() { PackageId = item.About.PackageId };
            if (String.IsNullOrEmpty(item.Path))
            {
                item.Alert.Add("Missing:" + item.Label);
            }
            yield return item;
        }

        LoadModsActive?.Invoke(LoadModsFromLocalRunning = false);
        GC.Collect();
    }
    public ModsConfigModel? LoadModsConfig(string? path = null)
    {
        string? aboutPath = path ?? FileHelper.GetModsConfig(_settingsService.Settings.PathDirGameConfig);
        if (String.IsNullOrEmpty(aboutPath)) return null;
        return XMLHelper.LoadModsConfig(aboutPath);
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
        //LocalDataListModel? modsData = XMLHelper.LoadLocalData(_settingsService.Settings.PathModData) ?? new LocalDataListModel();
        // LocalDataListModel modsData = JsonHelper.DeserializeModel<LocalDataListModel>(_settingsService.Settings.PathModData) ?? new LocalDataListModel();
        //  modsData.ModDataList.Clear();
        LocalDataListModel modsData = new LocalDataListModel();
        foreach (var item in modlist.Where(x => x.Data != null).Where(x => x.Data.IsNotNull()))
        {
            modsData.ModDataList.Add(item.Data);
        }     
        //XMLHelper.SaveLocalData(modsData, _settingsService.Settings.PathModData);
        JsonHelper.SerializeModel(modsData, _settingsService.Settings.PathModData);
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
            c.Add(("PackageId", m => m.About?.PackageId));
            c.Add(("SteamId", m => m.About?.SteamId));
            c.Add(("Name", m => m.Label));
            c.Add(("Path", m => m.Path));
            c.Add(("Autors", m => m.About?.Author));
            c.Add(("Versions", m => m.Versions));
            c.Add(("Color", m => m.Data?.Color));
            c.Add(("Color", m => m.Data?.Comment));
            c.Add(("Groups", m => m.Data?.Group));
            c.Add(("Groups", m => m.Data?.PackageGroup));
        });
    }
}

using RimKeeperModOrganizerLib.Extensions;
using RimKeeperModOrganizerLib.Helpers;
using RimKeeperModOrganizerLib.Models;
using System.Collections;
using System.Collections.Generic;
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

    public string? LastConfigLoad { get; set; }

    //public IEnumerable<ModModel> LoadMods(string? path = null) //.old
    //{
    //    if (LoadModsFromLocalRunning) yield break;
    //    LoadModsActive?.Invoke(LoadModsFromLocalRunning = true);

    //    ModsConfigModel? modsConfig = LoadModsConfig(path);
    //    if (modsConfig == null) yield break;

    //    LocalDataListModel modsData = JsonHelper.DeserializeModel<LocalDataListModel>(_settingsService.Settings.PathModData) ?? new LocalDataListModel();

    //    foreach (ModModel mod in FindRimWorldAllMods())
    //    {
    //        mod.Position = modsConfig?.Position(mod.About?.PackageId) ?? -1;
    //        mod.TrySet(modsData);
    //        if (mod.Position >= 0) mod.Selected = true;
    //        //  ModsList.Add(mod);
    //        yield return mod;
    //    }
    //    LoadModsActive?.Invoke(LoadModsFromLocalRunning = false);
    //    GC.Collect();
    //}
    //public IEnumerable<ModModel> LoadMods2(string? path = null)
    //{
    //    if (LoadModsFromLocalRunning) yield break;
    //    LoadModsActive?.Invoke(LoadModsFromLocalRunning = true);

    //    List<ModModel> modList = FindRimWorldAllMods().ToList();

    //    ModsConfigModel? modsConfig = LoadModsConfig(path);
    //    if (modsConfig != null)
    //    {
    //        foreach (var item in modsConfig.ActiveMods)
    //        {
    //            var modListItem = modList.FirstOrDefault(x => x?.About?.PackageId == item);
    //            if (modListItem == null)
    //            {
    //                modListItem = new ModModel() { About = new AboutModel() { PackageId = item } };
    //                modList.Add(modListItem);
    //            }
    //            modListItem.Position = modsConfig?.Position(item) ?? -1;
    //            if (modListItem.Position >= 0) modListItem.Selected = true;
    //        }
    //    }

    //    LocalDataListModel? modsData = JsonHelper.DeserializeModel<LocalDataListModel>(_settingsService.Settings.PathModData);
    //    if (modsData != null)
    //    {
    //        foreach (var item in modsData.ModDataList)
    //        {
    //            var modListItem = modList.FirstOrDefault(x => x?.About?.PackageId == item.PackageId);
    //            if (modListItem == null)
    //            {
    //                modListItem = new ModModel() { About = new AboutModel() { PackageId = item.PackageId } };
    //                modList.Add(modListItem);
    //            }
    //            modListItem.Data = item;
    //        }
    //    }

    //    foreach (ModModel item in modList)
    //    {
    //        if(item.Data == null) item.Data = new ModDataModel() { PackageId = item.About.PackageId };
    //        if (String.IsNullOrEmpty(item.Path))
    //        {
    //            item.Alert.Add("Missing:" + item.Label);
    //        }
    //        yield return item;
    //    }

    //    LoadModsActive?.Invoke(LoadModsFromLocalRunning = false);
    //    GC.Collect();
    //}

    ////public void LoadMods3(IList<ModModel> modCollecions, string? path = null)
    ////public IEnumerable<ModModel> LoadMods3(string? path = null)
    ////{
    ////    if (LoadModsFromLocalRunning) yield break;
    ////    LoadModsActive?.Invoke(LoadModsFromLocalRunning = true);

    ////    ModsConfigModel? modsConfig = LoadModsConfig(path);
    ////    LocalDataListModel? modsData = LoadModData();

    ////    IList<ModModel> modlist = new List<ModModel>();
    ////    ModModelSetConfig(modlist, modsConfig);
    ////    ModModelSetData(modlist, modsData);
    ////    ModModelSetLods(modlist, FindRimWorldAllMods());
    ////    foreach (var item in modlist)
    ////    {
    ////        Thread.Sleep(1);
    ////        yield return item;
    ////    }

    ////    LoadModsActive?.Invoke(LoadModsFromLocalRunning = false);
    ////    GC.Collect();
    ////    yield break;
    ////}
    //public IEnumerable<ModModel> LoadMods3_1(string? path = null)
    //{
    //    if (LoadModsFromLocalRunning) yield break;
    //    LoadModsActive?.Invoke(LoadModsFromLocalRunning = true);

    //    // 1. Pobierz dane konfiguracyjne (surowe dane)
    //    var modsConfig = LoadModsConfig(path);
    //    var modsData = LoadModData();
    //    var allFiles = FindRimWorldAllMods(); // To powinno być IEnumerable

    //    // Słownik do szybkiego łączenia danych po PackageId
    //    var dictionary = new Dictionary<string, ModModel>();

    //    // Funkcja pomocnicza do pobierania lub tworzenia modelu w słowniku
    //    ModModel GetOrAdd(string id)
    //    {
    //        if (!dictionary.TryGetValue(id, out var m))
    //        {
    //            m = new ModModel { About = new AboutModel { PackageId = id } };
    //            dictionary[id] = m;
    //        }
    //        return m;
    //    }

    //    // 2. Wypełnij słownik danymi z różnych źródeł
    //    if (modsConfig?.ActiveMods != null)
    //    {
    //        foreach (var id in modsConfig.ActiveMods)
    //        {
    //            var m = GetOrAdd(id);
    //            m.Position = modsConfig.Position(id);
    //            m.Selected = true;
    //        }
    //    }

    //    if (modsData?.ModDataList != null)
    //    {
    //        foreach (var data in modsData.ModDataList)
    //        {
    //            var m = GetOrAdd(data.PackageId);
    //            m.Data = data;
    //        }
    //    }

    //    // 3. Łączymy z plikami i od razu "wypychamy" do VM
    //    foreach (var fileMod in allFiles)
    //    {
    //        var id = fileMod.About?.PackageId;
    //        if (id == null) continue;

    //        var existing = GetOrAdd(id);
    //        // Aktualizujemy dane z plików
    //        existing.About = fileMod.About;
    //        existing.Path = fileMod.Path;
    //        existing.ThumbnailPath = fileMod.ThumbnailPath;
    //        existing.Local = fileMod.Local;

    //        // Kluczowe: Zwracamy moda natychmiast po złożeniu danych plikowych
    //        yield return existing;

    //        // Usuwamy ze słownika, by wiedzieć co zostało (opcjonalne)
    //        dictionary.Remove(id);
    //    }

    //    // 4. Zwróć resztę (np. mody aktywne, których nie ma fizycznie na dysku)
    //    foreach (var remaining in dictionary.Values)
    //    {
    //        yield return remaining;
    //    }

    //    LoadModsActive?.Invoke(LoadModsFromLocalRunning = false);
    //}

    //public IEnumerable<ModModel> LoadMods3_2(string? path = null)
    //{
    //    // 1. Ładowanie surowych danych (Metadata)
    //    var modsConfig = LoadModsConfig(path);
    //    var modsData = LoadModData();

    //    // 2. Indeksowanie Configa (PackageId -> Pozycja)
    //    // Używamy słownika, aby nie wołać .IndexOf() 500 razy w pętli
    //    var activeModsLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    //    if (modsConfig?.ActiveMods != null)
    //    {
    //        for (int i = 0; i < modsConfig.ActiveMods.Count; i++)
    //        {
    //            var id = modsConfig.ActiveMods[i].ToLower();
    //            if (!activeModsLookup.ContainsKey(id)) // Zabezpieczenie przed błędami w pliku XML
    //                activeModsLookup[id] = i;
    //        }
    //    }

    //    // 3. Indeksowanie Cache/Data (PackageId -> ModDataModel)
    //    var cacheLookup = modsData?.ModDataList?
    //        .Where(x => !string.IsNullOrEmpty(x.PackageId))
    //        .GroupBy(x => x.PackageId) // Na wypadek duplikatów w cache
    //        .ToDictionary(g => g.Key!, g => g.First(), StringComparer.OrdinalIgnoreCase)
    //        ?? new Dictionary<string, ModDataModel>();

    //    // 4. Dynamiczne skanowanie plików (Lazy Loading)
    //    // Używamy HashSet do śledzenia wysłanych ID (obsługa duplikatów fizycznych)
    //    var processedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    //    foreach (var mod in FindRimWorldAllMods()) // Zakładamy, że zwraca yield return ModModel
    //    {
    //        var packageId = mod.About?.PackageId;

    //        if (!string.IsNullOrEmpty(packageId))
    //        {
    //            // Łączenie z Configiem
    //            if (activeModsLookup.TryGetValue(packageId, out int pos))
    //            {
    //                mod.Selected = true;
    //                mod.Position = pos;
    //            }

    //            // Łączenie z Cache
    //            if (cacheLookup.TryGetValue(packageId, out var data))
    //            {
    //                mod.Data = data;
    //            }

    //            processedIds.Add(packageId);
    //        }

    //        yield return mod;
    //    }

    //    // 5. ETAP DODATKOWY: Mody wirtualne (widoczne w configu, ale brak ich na dysku)
    //    foreach (var activeId in activeModsLookup.Keys)
    //    {
    //        if (!processedIds.Contains(activeId))
    //        {
    //            yield return new ModModel
    //            {
    //                About = new AboutModel { PackageId = activeId },
    //                Selected = true,
    //                Position = activeModsLookup[activeId],
    //                // Flaga informująca, że mod jest "duchem"
    //                Local = false
    //            };
    //        }
    //    }
    //}

    public IEnumerable<ModModel> LoadModMetaData(string? path = null)
    {
        if (LoadModsFromLocalRunning) yield break;
        LoadModsActive?.Invoke(LoadModsFromLocalRunning = true);

        ModsConfigModel? modsConfig = LoadModsConfig(path);
        LocalDataListModel? modsData = LoadModData();

        foreach (var packageId in modsConfig.ActiveMods)
        {
            var newMod = modsConfig.Make(packageId);
            newMod.Data = modsData?.ModDataList.FirstOrDefault(x => x.PackageId == packageId);
            yield return newMod;
        }
        foreach (var item in modsData.ModDataList.Where(w => w.PackageId != null && !modsConfig.ActiveMods.Contains(w.PackageId)))
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
            c.Add(("Autors", m => m.About?.Authors));
            c.Add(("Versions", m => m.Versions));
            c.Add(("Color", m => m.Data?.Color));
            c.Add(("Color", m => m.Data?.Comment));
            c.Add(("Groups", m => m.Data?.Group));
            c.Add(("Groups", m => m.Data?.PackageGroup));
        });
    }
}
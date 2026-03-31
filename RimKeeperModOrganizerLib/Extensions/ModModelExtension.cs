using RimKeeperModOrganizerLib.Models;
namespace RimKeeperModOrganizerLib.Extensions;

public static class ModModelExtension
{
    public static void AddOrUpdate(this IList<ModModel> modlist, IEnumerable<ModModel> items)
    {
        foreach (var item in items)
            AddOrUpdate(modlist, item);
    }
    public static void AddOrUpdate(this IList<ModModel> modlist, ModModel? item)
    {
        if (item == null) return;
        var existingMod = modlist.FirstOrDefault(x => x.About?.PackageId == item.About?.PackageId);
        if (existingMod != null)
        {
            existingMod.About = item.About;
            existingMod.Path = item.Path;
            existingMod.ThumbnailPath = item.ThumbnailPath;
            existingMod.Local = item.Local;
        }
        else
        {
            modlist.Add(item);
        }
    }

    public static void AddOrUpdate(this IList<ModModel> modlist, LocalDataListModel? item)
    {
        if (item == null) return;
        modlist.AddOrUpdate(item.ModDataList);
    }
    public static void AddOrUpdate(this IList<ModModel> modlist, IEnumerable<ModDataModel> items)
    {
        foreach (var item in items)
            AddOrUpdate(modlist, item);
    }
    public static void AddOrUpdate(this IList<ModModel> modlist, ModDataModel? item)
    {
        if (item == null) return;
        var existingMod = modlist.FirstOrDefault(x => x.About?.PackageId == item.PackageId);
        if (existingMod != null)
            existingMod.Data = item;
        else
            modlist.Add(item.Make());
    }

    public static void AddOrUpdate(this IList<ModModel> modlist, ModsConfigModel? modsConfig)
    {
        if (modsConfig == null) return;
        if (modsConfig?.ActiveMods == null) return;
        foreach (var mod in modlist)
            mod.Position = modsConfig.Position(mod.About?.PackageId);
        foreach (var packageId in modsConfig.ActiveMods)
            if (!modlist.Any(x => x.About?.PackageId == packageId))
                modlist.Add(modsConfig.Make(packageId));
    }

    public static ModModel Make(this ModDataModel data) => new ModModel
    {
        About = new AboutModel { PackageId = data.PackageId },
        Data = data
    };
    public static ModModel Make(this ModsConfigModel modsConfig, string packageId) => new ModModel
    {
        About = new AboutModel { PackageId = packageId },
        Position = modsConfig.Position(packageId),
    };
}

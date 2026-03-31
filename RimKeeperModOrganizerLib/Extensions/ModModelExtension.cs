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
            existingMod.Update(item.MakeData());
        }
        else
        {
            modlist.Add(item.MakeData());
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
            existingMod.Update(item);
        else
            modlist.Add(item.Make());
    }

    public static void AddOrUpdate(this IList<ModModel> modlist, ModsConfigModel? modsConfig)
    {
        if (modsConfig == null) return;
        if (modsConfig?.ActiveMods == null) return;
        foreach (var mod in modlist)
            mod.Update(modsConfig);
        foreach (var packageId in modsConfig.ActiveMods)
            if (!modlist.Any(x => x.About?.PackageId == packageId))
                modlist.Add(modsConfig.Make(packageId));
    }

    public static ModModel MakeData(this ModModel data)
    {
        if (!string.IsNullOrEmpty(data.About?.PackageId))
            data.Data = new ModDataModel { PackageId = data.About.PackageId };
        return data;
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
    public static ModModel Make(this string packageId) => new ModModel
    {
        About = new AboutModel { PackageId = packageId },
    };
    public static void Update(this ModModel data, ModModel item)
    {
        data.About = item.About;
        data.Path = item.Path;
        data.ThumbnailPath = item.ThumbnailPath;
        data.Local = item.Local;
    }
    public static ModModel Update(this ModModel data, ModDataModel item)
    {
        data.Data = item;
        return data;
    }
    public static ModModel Update(this ModModel data, ModsConfigModel modsConfig)
    {
        data.Position = modsConfig.Position(data.About?.PackageId);
        return data;
    }

    public static bool IsNotNull(this ModDataModel? model) 
        => !IsNull(model);
    public static bool IsNull(this ModDataModel? model)
        => model == null || (
            string.IsNullOrEmpty(model.Color)
            && !model.Groups.Any()
            && !model.PackageGroups.Any()
            && string.IsNullOrEmpty(model.Comment)
        );

    public static void Clear(this ModDataModel model)
    {
        model.Color = null;
        model.Comment = null;
        model.Groups.Clear();
        model.PackageGroups.Clear();
    }
}

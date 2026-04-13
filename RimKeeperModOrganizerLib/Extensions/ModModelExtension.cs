using RimKeeperModOrganizerLib.Models;
using System.Data;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
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
        var existingMods = modlist.FindByKey(item).ToList();
        if (existingMods.Any())
        {
            foreach (var ritem in existingMods)
            {
                ritem.Update(item.MakeData());
            }
        }
        else
        {
            modlist.Add(item.MakeData());
        }
        //var tt = modlist.FirstOrDefault(x => x.About?.PackageId == item.About?.PackageId);
        ////var existingMod = modlist.FirstOrDefault(x => x.About?.PackageId == item.About?.PackageId);
        //var existingMod = modlist.FirstOrDefault(x => x.Key == item.Key);
        //if (existingMod == null)
        //{
        //    existingMod = modlist.FirstOrDefault(x => x.Key == item.GetKeyID());
        //    //existingMod = modlist.FirstOrDefault(x => x.About?.PackageId == item.About?.PackageId);
        //}
        //if (existingMod != null)
        //{
        //    existingMod.Update(item.MakeData());
        //}
        //else
        //{
        //    modlist.Add(item.MakeData());
        //}
    }
    public static void AddOrUpdate(this IList<ModModel> modlist, LocalDataListModel2? item)
    {
        if (item == null) return;
        modlist.AddOrUpdate(item.ModDataList);
    }
    public static void AddOrUpdate(this IList<ModModel> modlist, IEnumerable<KeyValuePair<string, ModDataModel>> items)
    {
        foreach (var item in items)
            AddOrUpdate(modlist, item);
    }
    public static void AddOrUpdate(this IList<ModModel> modlist, KeyValuePair<string, ModDataModel> item)
    {
        //if (item == null) return;
        var existingMods = modlist.FindByKey(item.Key).ToList();
        if (existingMods.Any())
        {
            foreach (var ritem in existingMods)
            {
                ritem.Update(item.Value);
            }
        }
        else
        {
            modlist.Add(item.Make());
        }
        ////var existingMod = modlist.FirstOrDefault(x => x.About?.PackageId == item.PackageId);
        //var existingMod = modlist.FirstOrDefault(x => x.Key == item.Key);
        //if (existingMod == null)
        //{
        //    existingMod = modlist.FirstOrDefault(x => x.Key == item.GetKeyID());
        //}
        //if (existingMod != null)
        //    existingMod.Update(item);
        //else
        //    modlist.Add(item.Make());
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
        //if (!string.IsNullOrEmpty(data.About?.PackageId))
        //data.Data = new ModDataModel { PackageId = data.About.PackageId, Key = data.Key };
        //data.Data = new ModDataModel { Key = data.Key };
        return data.Update(new ModDataModel());
    }


    
    public static ModModel Make(this KeyValuePair<string, ModDataModel> data) => new ModModel
    {
        ModId = data.Key,
        //About = new AboutModel { PackageId = data.PackageId },
        Location = ModLocation.MetaData,
        Data = data.Value,
    };
    //public static ModModel Make(this ModDataModel data) => new ModModel
    //{
    //    Key = data.Key,
    //    //About = new AboutModel { PackageId = data.PackageId },
    //    Data = data,
    //};
    public static ModModel Make(this ModDataModel data, string key)
    {
        //data.Key = key;
        return new ModModel
        {
            //About = new AboutModel { PackageId = data.PackageId },
            Data = data,
            ModId = key,
        };
    }
    //public static ModModel Make(this ModDataModel data, string key)
    //{
    //    data.Key = key;
    //    return new ModModel
    //    {
    //        About = new AboutModel { PackageId = data.PackageId },
    //        Data = data,
    //        Key = key,
    //    };
    //}
    public static ModModel Make(this ModsConfigModel modsConfig, string packageId) => new ModModel
    {
        About = new AboutModel { PackageId = packageId },
        Position = modsConfig.Position(packageId),
        ModId = packageId,
    };
    public static ModModel Make(this string packageId) => new ModModel
    {
        About = new AboutModel { PackageId = packageId },
        ModId = packageId,
    };
    public static void Update(this ModModel data, ModModel item)
    {
        data.About = item.About;
        data.Path = item.Path;
        data.ThumbnailPath = item.ThumbnailPath;
        //data.Local = item.Local;
        data.Location = item.Location;
        data.ModId = data.GenKey();
       // if(data.Data != null) data.Data.Key = data.Key;
    }
    public static ModModel Update(this ModModel data, ModDataModel item)
    {
      //  data.Key = item.Key;
        data.Data = item;
        return data;
    }
    public static ModModel Update(this ModModel data, ModsConfigModel modsConfig)
    {
        data.Position = modsConfig.Position(data.About?.PackageId);
        return data;
    }

    public static string GetKeyID(this KeyValuePair<string, ModDataModel> model) => GetKeyID(model.Key);
    //public static string GetKeyID(this ModDataModel model) => GetKeyID(model.Key);
    public static string GetKeyID(this ModModel model) => GetKeyID(model.ModId);
    public static string GetKeyID(string model) => model.Split(':')[0];
    public static string GenKey(this ModModel model) => GenKey(model.About?.PackageId,model.Path);
    public static string GenKey(string? packageId, string? path) => packageId + ":" + Path.GetFileName(path);
    public static IEnumerable<ModModel> FindByKey(this IEnumerable<ModModel> models, ModModel item)
    {
        bool found = false;
        foreach (var model in models.Where(x => x.ModId == item.ModId))
        {
            found = true;
            yield return model;
        }
        if (found) yield break;
        string keyid = item.GetKeyID();
        foreach (var model in models.Where(x => x.ModId == keyid))
        {
            yield return model;
        }
    }
    public static IEnumerable<ModModel> FindByKey(this IEnumerable<ModModel> models, string item)
    {
        bool found = false;
        foreach (var model in models.Where(x => x.ModId == item))
        {
            found = true;
            yield return model;
        }
        if (found) yield break;
        string keyid = GetKeyID(item);
        foreach (var model in models.Where(x => x.ModId == keyid))
        {
            yield return model;
        }
    }
    public static ModDataModel? FindByKey(this Dictionary<string, ModDataModel> models, string modid)
    {
        if (models.TryGetValue(modid, out var value))
            return value;

        foreach (var kv in models)
            if (kv.GetKeyID() == modid)
                return kv.Value;

        return null;
    }
    //public static IEnumerable<ModModel> FindByKey(this IEnumerable<ModModel> models, ModDataModel item)
    //{
    //    bool found = false;
    //    foreach (var model in models.Where(x => x.Key == item.Key))
    //    {
    //        found = true;
    //        yield return model;
    //    }
    //    if (found) yield break;
    //    string keyid = item.GetKeyID();
    //    foreach (var model in models.Where(x => x.Key == keyid))
    //    {
    //        yield return model;
    //    }
    //}


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

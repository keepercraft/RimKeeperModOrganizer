using RimKeeperModOrganizerLib.Models;
namespace RimKeeperModOrganizerLib.Extensions;

public static class ListExtension
{
    public static void SortCollectionByConfig(this IList<ModModel>? mods, ModsConfigModel? config)
    {
        if (config == null || mods == null || config.ActiveMods == null) return;
        for (int i = 0; i < config.ActiveMods.Count; i++)
        {
            if (mods.FirstOrDefault(x => x.Data?.PackageId == config.ActiveMods[i]) is ModModel mod)
            {
                int indexInMods = mods.IndexOf(mod);
                if (indexInMods != i)
                {
                    var item = mods[indexInMods];
                    mods.RemoveAt(indexInMods);
                    mods.Insert(i, item);
                }
            }
        }
    }

    public static void ModListValidation(this IList<ModModel> modlist)
    {
        int index = 0;
        foreach (var mod in modlist)
        {
            mod.Alert.Clear();

            if (String.IsNullOrEmpty(mod.Path))
            {
                mod.Alert.Add("Missing:" + mod.Label);
            }

            if (string.IsNullOrEmpty(mod.About?.PackageId)) continue;

            if (mod.About?.ModDependencies is { } mod_deps)
                foreach (var mod_dep in mod_deps)
                {
                    if (string.IsNullOrEmpty(mod_dep.PackageId)) continue;
                    if (!modlist.Any(a => a.About?.PackageId == mod_dep.PackageId))
                    {
                        mod.Alert.Add("Dependency:" + mod_dep.PackageId);
                    }
                }

            if (mod.About?.IncompatibleWith is { } mod_incom_deps)
                foreach (var mod_dep_id in mod_incom_deps)
                {
                    if (string.IsNullOrEmpty(mod_dep_id)) continue;
                    if (modlist.Any(a => a.About?.PackageId == mod_dep_id))
                    {
                        mod.Alert.Add("IncompatibleWith:" + mod_dep_id);
                    }
                }

            if (mod.About?.LoadAfter is { } mod_after_deps)
                foreach (var mod_dep_id in mod_after_deps)
                {
                    if (string.IsNullOrEmpty(mod_dep_id)) continue;
                    if (modlist.Skip(index + 1).Where(a => a.About?.PackageId == mod_dep_id).Any(a => a.About?.PackageId == mod_dep_id))
                    {
                        mod.Alert.Add("LoadAfter:" + mod_dep_id);
                    }
                }

            if (mod.About?.LoadBefore is { } mod_before_deps)
                foreach (var mod_dep_id in mod_before_deps)
                {
                    if (string.IsNullOrEmpty(mod_dep_id)) continue;
                    if (modlist.Take(index).Where(a => a.About?.PackageId == mod_dep_id).Any(a => a.About?.PackageId == mod_dep_id))
                    {
                        mod.Alert.Add("LoadBefore:" + mod_dep_id);
                    }
                }

            mod.RaisePropertyChanged(nameof(ModModel.HasAlert));
            index++;
        }
    }
}

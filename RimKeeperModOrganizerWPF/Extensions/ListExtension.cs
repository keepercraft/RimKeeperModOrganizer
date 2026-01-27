using RimKeeperModOrganizerLib.Models;
using System.Collections.ObjectModel;

namespace RimKeeperModOrganizerWPF.Extensions;

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
}

using KeeperBaseLib.Model;
using RimKeeperModOrganizerLib.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace RimKeeperModOrganizerWPF.Extensions;

public static class ListExtension
{
    //public static void SortCollectionByConfig(this IList<ModModel>? mods, ModsConfigModel? config)
    //{
    //    if (config == null || mods == null || config.ActiveMods == null) return;
    //    for (int i = 0; i < config.ActiveMods.Count; i++)
    //    {
    //        if (mods.FirstOrDefault(x => x.Data?.PackageId == config.ActiveMods[i]) is ModModel mod)
    //        {
    //            int indexInMods = mods.IndexOf(mod);
    //            if (indexInMods != i)
    //            {
    //                var item = mods[indexInMods];
    //                mods.RemoveAt(indexInMods);
    //                mods.Insert(i, item);
    //            }
    //        }
    //    }
    //}

    public static void SyncProperties<T>(
        this NotifyCollectionChangedEventArgs e, 
        PropertyChangedEventHandler handler, 
        Func<T, INotifyPropertyChanged?> selector)
    {
        if (e.OldItems != null)
            foreach (T oldItem in e.OldItems)
                selector(oldItem)?.PropertyChanged -= handler;
        if (e.NewItems != null)
            foreach (T newItem in e.NewItems)
                selector (newItem)?.PropertyChanged += handler;
    }

    public static void ClearSyncProperties<T>(
        this ObservableCollection<T> collection,
        PropertyChangedEventHandler handler,
        Func<T, INotifyPropertyChanged?> selector)
    {
        foreach (T item in collection)
            selector(item)?.PropertyChanged -= handler;
        collection.Clear();
    }
}

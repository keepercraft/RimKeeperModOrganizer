using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
namespace RimKeeperModOrganizerAvalonia.Extensions;

public static class ListExtension
{
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

    public static bool SyncWith<T>(this IList<T> target, IEnumerable<T> source)
    {
        var srcSet = source as HashSet<T> ?? source.ToHashSet();
        bool changed = false;
        for (int i = target.Count - 1; i >= 0; i--)
            if (!srcSet.Contains(target[i]))
            {
                target.RemoveAt(i);
                changed = true;
            }

        foreach (var item in srcSet)
            if (!target.Contains(item))
            {
                target.Add(item);
                changed = true;
            }
        return changed;
    }
}

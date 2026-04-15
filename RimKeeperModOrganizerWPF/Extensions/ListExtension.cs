using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
namespace RimKeeperModOrganizerWPF.Extensions;

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
}

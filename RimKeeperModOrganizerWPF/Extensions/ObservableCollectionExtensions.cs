using KeeperDataGrid.Models;
using RimKeeperModOrganizerLib.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RimKeeperModOrganizerWPF.Extensions;

public static class ObservableCollectionExtensions
{
    public static void SortBy<T, TKey>(
        this ObservableCollection<T> collection,
        Func<T, TKey> keySelector,
        Func<T, bool>? keyFilter = null,
        bool ascending = true)
    {
        if (collection == null || collection.Count <= 1)
            return;

        List<T> sorted;
        if (ascending)
            sorted = collection.OrderBy(keySelector).ToList();
        else
            sorted = collection.OrderByDescending(keySelector).ToList();

        // Przestawianie elementów zamiast Clear + Add (bardziej optymalne)
        for (int i = 0; i < sorted.Count; i++)
        {
            var item = sorted[i];
            if (keyFilter != null && !keyFilter(item)) continue;
            int oldIndex = collection.IndexOf(item);
            if (oldIndex != i)
                collection.Move(oldIndex, i);
        }
    }

    public static void InsertInOrder<T, TKey>(this IList<T> collection, T newItem, Func<T, TKey?> keySelector) where TKey : struct, IComparable
    {
        int indexToInsert = collection.Count;
        TKey? newKey = keySelector(newItem);

        // Jeśli nowy element ma pozycję null, ląduje na końcu (domyślny indexToInsert)
        if (newKey.HasValue)
        {
            indexToInsert = collection.Count; // Reset na wypadek, gdyby wszystkie obecne miały wartości

            for (int i = 0; i < collection.Count; i++)
            {
                TKey? currentKey = keySelector(collection[i]);
                if (!currentKey.HasValue || currentKey.Value.CompareTo(newKey.Value) > 0)
                {
                    indexToInsert = i;
                    break;
                }
            }
        }
        collection.Insert(indexToInsert, newItem);
    }
}
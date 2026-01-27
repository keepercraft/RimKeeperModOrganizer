using System.Collections.ObjectModel;

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
}
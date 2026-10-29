using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
namespace RimKeeperModOrganizerAvalonia.Extensions;

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


    public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }

    /// <summary>
    /// Wstawia element do kolekcji w odpowiednie miejsce, zachowując jej porządek.
    /// </summary>
    /// <typeparam name="T">Typ elementów w kolekcji.</typeparam>
    /// <param name="list">Kolekcja, do której następuje wstawienie (musi implementować IList<T>).</param>
    /// <param name="item">Wstawiany element.</param>
    /// <param name="descending">Jeśli true, sortuje malejąco; jeśli false (domyślnie), rosnąco.</param>
    /// <param name="comparer">Opcjonalny comparer do porównywania elementów.</param>
    //public static void InsertSorted<T>(
    //    this IList<T> list,
    //    T item,
    //    bool descending = false,
    //    IComparer<T>? comparer = null)
    //{
    //    if (list == null) throw new ArgumentNullException(nameof(list));
    //    comparer ??= Comparer<T>.Default;
    //    int index = 0;

    //    while (index < list.Count)
    //    {
    //        int comparisonResult = comparer.Compare(list[index], item);
    //        if (descending ? comparisonResult < 0 : comparisonResult > 0)
    //        {
    //            break;
    //        }

    //        index++;
    //    }
    //    list.Insert(index, item);
    //}
    public static void InsertSorted<T>(this IList<T> list, T item, bool descending = false) where T : IComparable<T>
    {
        if (list == null) throw new ArgumentNullException(nameof(list));
        int index = 0;
        while (index < list.Count)
        {
            int comparisonResult = list[index].CompareTo(item);
            if (descending ? comparisonResult < 0 : comparisonResult > 0)  break;
            index++;
        }
        list.Insert(index, item);
    }
}
using System.ComponentModel;
namespace KeeperDataGrid.Extensions;

public static class CollectionExtension
{
    public static void CombineFilters(this ICollectionView view, Func<object, bool> filter)
    {
        var existingFilter = view.Filter;
        view.Filter = (item) =>
        {
            bool passesExisting = existingFilter == null || existingFilter(item);
            if (!passesExisting) return false;
            return filter(item);
        };
    }

    public static void CombineFilters(this ICollectionView view, IEnumerable<Func<object, bool>> filters)
    {
        var existingFilter = view.Filter;
        view.Filter = (item) =>
        {
            bool passesExisting = existingFilter == null || existingFilter(item);
            if (!passesExisting) return false;
            foreach (var filter in filters)
            {
                if(!filter(item)) 
                    return false;
            }
            return true;
        };
    }
}

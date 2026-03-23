using System.ComponentModel;
using System.Linq.Expressions;
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

    public static string GetPropertyName<T>(this Expression<Func<T, object>> expr)
    {
        if (expr.Body is MemberExpression m) return m.Member.Name;
        if (expr.Body is UnaryExpression u && u.Operand is MemberExpression um) return um.Member.Name;
        throw new ArgumentException("Invalid expression");
    }
}

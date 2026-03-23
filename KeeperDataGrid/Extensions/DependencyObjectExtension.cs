using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace KeeperDataGrid.Extensions;

public static class DependencyObjectExtension
{
    public static bool IsIn<T>(this DependencyObject? obj) where T : DependencyObject
    {
        while (obj != null)
        {
            if (obj is T) return true;
            obj = VisualTreeHelper.GetParent(obj);
        }
        return false;
    }

    public static T? FindParent<T>(this DependencyObject? obj) where T : DependencyObject
    {
        while (obj != null)
        {
            if (obj is T t)
            {
                return t;
            }
            obj = VisualTreeHelper.GetParent(obj);
        }
        return null;
    }

}

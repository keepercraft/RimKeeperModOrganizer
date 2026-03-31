using System.Collections.ObjectModel;
using System.Collections.Specialized;
namespace RimKeeperModOrganizerWPF.Models;

public class BulkObservableCollection<T> : ObservableCollection<T>
{
    public void AddRange(IEnumerable<T> items)
    {
        this.CheckReentrancy();
        foreach (var item in items)
        {
            this.Items.Add(item);
        }
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}
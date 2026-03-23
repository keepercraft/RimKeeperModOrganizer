using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace KeeperDataGrid.Models;

public class KeeperPropertyModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    public virtual void RaisePropertyChanged() => GetType().GetProperties().All(c => { RaisePropertyChanged(c.Name); return true; });
    public virtual void RaisePropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    public void OnPropertyChanged([CallerMemberName] string name = null) => RaisePropertyChanged(name);
}
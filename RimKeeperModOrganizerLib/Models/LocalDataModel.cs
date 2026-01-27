using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace RimKeeperModOrganizerLib.Models;

public class LocalDataListModel 
{
    public List<ModDataModel> ModDataList { get; set; } = new List<ModDataModel>();
}

public class ModDataModel : INotifyPropertyChanged
{
    public ModDataModel()
    {
        Groups.CollectionChanged += Groups_CollectionChanged;
    }
    private void Groups_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Group)));
    }
    public ObservableCollection<string> Groups { get; set; } = new();
    public string? Group => (Groups != null && Groups.Any()) ? string.Join(",", Groups) : null;


    public string? PackageId { get; set; }

    private string? _Color = null;
    public string? Color
    {
        get => _Color;
        set
        {
            if (_Color != value)
            {
                _Color = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Color)));
            }
        }
    }

    //public bool NotNull => !string.IsNullOrEmpty(Color) || !string.IsNullOrEmpty(Group);
    public bool NotNull => !string.IsNullOrEmpty(Color) || Groups.Any();

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

}
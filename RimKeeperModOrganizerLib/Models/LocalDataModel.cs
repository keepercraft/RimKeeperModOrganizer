using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;
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
        PackageGroups.CollectionChanged += PackageGroups_CollectionChanged;
    }

    public ObservableCollection<string> Groups { get; set; } = new();
    [JsonIgnore]
    public string? Group => (Groups != null && Groups.Any()) ? string.Join(",", Groups) : null;
    private void Groups_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Group)));

    public ObservableCollection<string> PackageGroups { get; set; } = new();
    [JsonIgnore]
    public string? PackageGroup => (PackageGroups != null && PackageGroups.Any()) ? string.Join(",", PackageGroups) : null;
    private void PackageGroups_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PackageGroup)));

    private string? _PackageId = null;
    public string? PackageId
    {
        get => _PackageId;
        set
        {
            if (_PackageId != value)
            {
                _PackageId = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PackageId)));
            }
        }
    }

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

    private string? _Comment = null;
    public string? Comment
    {
        get => _Comment;
        set
        {
            if (_Comment != value)
            {
                _Comment = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Comment)));
            }
        }
    }
   
    [JsonIgnore]
    public bool NotNull => !string.IsNullOrEmpty(Color) || Groups.Any();
    //public bool NotNull => !string.IsNullOrEmpty(Color) || !string.IsNullOrEmpty(Group);

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
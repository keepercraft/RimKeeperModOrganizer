using KeeperBaseLib.Model;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json.Serialization;
namespace RimKeeperModOrganizerLib.Models;

public class LocalDataListModel 
{
    public List<ModDataModel> ModDataList { get; set; } = new List<ModDataModel>();
}

public class ModDataModel : PropertyModel
{
    private ObservableCollection<string> _groups = new();
    public ObservableCollection<string> Groups
    {
        get => _groups;
        set
        {
            if (ReferenceEquals(_groups, value))  return;
            _groups.CollectionChanged -= Groups_CollectionChanged;
            _groups = value ?? new();
            _groups.CollectionChanged += Groups_CollectionChanged;
            RaisePropertyChanged(nameof(Group));
        }
    }
    private void Groups_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => RaisePropertyChanged(nameof(Group));
    [JsonIgnore]
    public string? Group => _groups.Count > 0 ? string.Join(",", _groups) : null;

    private ObservableCollection<string> _packageGroups = new();
    public ObservableCollection<string> PackageGroups
    {
        get => _packageGroups;
        set
        {
            if (ReferenceEquals(_packageGroups, value)) return;
            _packageGroups.CollectionChanged -= PackageGroups_CollectionChanged;
            _packageGroups = value ?? new();
            _packageGroups.CollectionChanged += PackageGroups_CollectionChanged;
            RaisePropertyChanged(nameof(PackageGroup));
        }
    }
    private void PackageGroups_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => RaisePropertyChanged(nameof(PackageGroup));
    [JsonIgnore]
    public string? PackageGroup => _packageGroups.Count > 0 ? string.Join(",", _packageGroups) : null;

    private string? _PackageId = null;
    public string? PackageId
    {
        get => _PackageId;
        set
        {
            if (_PackageId == value) return;          
            _PackageId = value;
            OnPropertyChanged();          
        }
    }

    private string? _Color = null;
    public string? Color
    {
        get => _Color;
        set
        {
            if (_Color == value) return;
            _Color = value;
            OnPropertyChanged();           
        }
    }

    private string? _Comment = null;
    public string? Comment
    {
        get => _Comment;
        set
        {
            if (_Comment == value) return;          
            _Comment = value;
            OnPropertyChanged();          
        }
    }
}
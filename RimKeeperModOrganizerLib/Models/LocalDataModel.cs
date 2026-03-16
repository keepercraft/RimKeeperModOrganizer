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
    public ModDataModel()
    {
        Groups.CollectionChanged += Groups_CollectionChanged;
        PackageGroups.CollectionChanged += PackageGroups_CollectionChanged;
    }

    public ObservableCollection<string> Groups { get; set; } = new();
    [JsonIgnore]
    public string? Group => (Groups != null && Groups.Any()) ? string.Join(",", Groups) : null;
    private void Groups_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => RaisePropertyChanged(nameof(Group));

    public ObservableCollection<string> PackageGroups { get; set; } = new();
    [JsonIgnore]
    public string? PackageGroup => (PackageGroups != null && PackageGroups.Any()) ? string.Join(",", PackageGroups) : null;
    private void PackageGroups_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => RaisePropertyChanged(nameof(PackageGroup));

    private string? _PackageId = null;
    public string? PackageId
    {
        get => _PackageId;
        set
        {
            if (_PackageId != value)
            {
                _PackageId = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }
    }
}

public static class ModDataModelExtension
{
    public static bool IsNotNull(this ModDataModel? model)
        => !IsNull(model);
    public static bool IsNull(this ModDataModel? model) 
        => model == null || (
            string.IsNullOrEmpty(model.Color) 
            && !model.Groups.Any() 
            && !model.PackageGroups.Any() 
            && string.IsNullOrEmpty(model.Comment)
        );

    public static void Clear(this ModDataModel model)
    {
        model.Color = null;
        model.Comment = null;
        model.Groups.Clear();
        model.PackageGroups.Clear();
    }
}
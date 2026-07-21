using KeeperBaseSharedLib.Models;
using RimKeeperModOrganizerLib.Extensions;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json.Serialization;
namespace RimKeeperModOrganizerLib.Models;

public class LocalDataListModel 
{
    public List<ModDataModel> ModDataList { get; set; } = new();
}

public class LocalDataListModel2
{
    public Dictionary<string,ModDataModel> ModDataList { get; set; } = new();
}

public class ModDataModel : PropertyModel
{
    public ModDataModel() 
    {
        Groups = new();
    }
    //public string Key { get; set; } = ":";
    //public string? PackageId { get; set; } //key to change

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

public class ModDataCopyModel
{
    public string? Color { get; set; }
    public string? Comment { get; set; }
    public string[]? Groups { get; set; }
    public string[]? PackageGroups { get; set; }
}

[Flags]
public enum ModDataCopySelection : byte
{
    All = Color | Comment | Groups,// | PackageGroups,
    None = 0,
    Color = 1,
    Comment = 2,
    Groups = 4,
    PackageGroups = 8,
}

public static class ModDataCopyExtension
{
    public static void Clear(this ModDataCopyModel context)
    {
        context.Color = null;
        context.Comment = null;
        context.Groups = null;
        context.PackageGroups = null;
    }

    public static void Cut(this ModDataCopyModel context, ModModel? mod, ModDataCopySelection selection = ModDataCopySelection.All)
    {
        if (mod == null) return;
        context.Copy(mod, selection);
        if (mod.Data == null) return;
        if (selection.HasFlag(ModDataCopySelection.Color)) mod.Data.Color = null;
        if (selection.HasFlag(ModDataCopySelection.Comment)) mod.Data.Comment = null;
        if (selection.HasFlag(ModDataCopySelection.Groups)) mod.Data.Groups.Clear();
        if (selection.HasFlag(ModDataCopySelection.PackageGroups)) mod.Data.PackageGroups.Clear();
    }

    public static void Copy(this ModDataCopyModel context, ModModel? mod, ModDataCopySelection selection = ModDataCopySelection.All)
    {
        if (mod == null) return;
        context.Color = selection.HasFlag(ModDataCopySelection.Color) ? mod.Data?.Color : null;
        context.Comment = selection.HasFlag(ModDataCopySelection.Comment) ? mod.Data?.Comment : null;
        context.Groups = selection.HasFlag(ModDataCopySelection.Groups) ? mod.Data?.Groups.ToArray() : null;
        context.PackageGroups = selection.HasFlag(ModDataCopySelection.PackageGroups) ? mod.Data?.PackageGroups.ToArray() : null;
    }

    public static void Paste(this ModDataCopyModel context, ModModel? mod, ModDataCopySelection selection = ModDataCopySelection.All)
    {
        if (mod == null) return;
        if (mod.Data == null) mod.MakeData();
        if (selection.HasFlag(ModDataCopySelection.Color)) mod.Data.Color = context.Color ;
        if (selection.HasFlag(ModDataCopySelection.Comment)) mod.Data.Comment = context.Comment;
        if (selection.HasFlag(ModDataCopySelection.Groups))
        {
            mod.Data.Groups.Clear();
            if (context.Groups != null)
            {
                foreach (var group in context.Groups)
                {
                    mod.Data.Groups.Add(group);
                }
            }
        }
        if (selection.HasFlag(ModDataCopySelection.PackageGroups))
        {
            mod.Data.PackageGroups.Clear();
            if (context.PackageGroups != null)
            {
                foreach (var packageGroup in context.PackageGroups)
                {
                    mod.Data.PackageGroups.Add(packageGroup);
                }
            }
        }
    }
    public static void Paste(this ModDataCopyModel context, IEnumerable<ModModel> mods, ModDataCopySelection selection = ModDataCopySelection.All)
    {
        foreach (var mod in mods)
        {
            Paste(context, mod, selection);
        }
    }
}
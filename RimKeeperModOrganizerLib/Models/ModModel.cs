using KeeperBaseSharedLib.Models;
using RimKeeperModOrganizerLib.Extensions;
using RimKeeperModOrganizerLib.Helpers;
using System.ComponentModel;
namespace RimKeeperModOrganizerLib.Models;

public class ModModel : PropertyModel
{
    public string ModId { get; set; } = string.Empty;
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(ModId);
    public override bool Equals(object? obj) => obj is ModModel m && StringComparer.Ordinal.Equals(ModId, m.ModId);

    public ModModel()
    {
        ModId = this.GenKey();
    }
    public ModModel(string path, ModLocation location = ModLocation.Unknow, bool local = false)
    {
        Path = path;
        if (!Directory.Exists(Path)) return;
        About = XMLHelper.LoadAboutFromModPath(path);
        if (About == null) return;
        ThumbnailPath = FileHelper.GetModPreview(path);
        About.SteamId = FileHelper.GetModPublishID(path);
        Location = location;
        //Local = local;
        this.MakeData();
        ModId = this.GenKey();
    }

    public string? Path { get; set; }
    public string? ThumbnailPath { get; set; }

    public string PackageId => About?.PackageId ?? About?.PackageId ?? this.GetKeyID();
    public string Label => About?.Name ?? About?.PackageId ?? ModId ?? "??";
    public string Versions => About?.SupportedVersions != null && About.SupportedVersions.Any() ? string.Join(",", About.SupportedVersions.OrderBy(v => v)) : "";
    public string? SteamLink => String.IsNullOrEmpty(About?.SteamId) ? null : string.Format(@"https://steamcommunity.com/sharedfiles/filedetails/?id={0}", About?.SteamId);

    private ModLocation _location { get; set; }
    public ModLocation Location
    {
        get => _location;
        set
        {
            if (ReferenceEquals(_location, value)) return;
            _location = value;
            OnPropertyChanged();
        }
    }

    private AboutModel? _about = null;
    public AboutModel? About
    {
        get => _about;
        set
        {
            if (ReferenceEquals(_about, value)) return;
            _about = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Label));
            OnPropertyChanged(nameof(Versions));
            OnPropertyChanged(nameof(SteamLink));
        }
    }

    private void OnSubModelPropertyChanged(object? sender, PropertyChangedEventArgs e) => OnPropertyChanged(nameof(Data));
    private ModDataModel? _data = null;
    public ModDataModel? Data
    {
        get => _data;
        set
        {
            if (ReferenceEquals(_data, value)) return;
            if (_data != null) _data.PropertyChanged -= OnSubModelPropertyChanged;
            _data = value;
            if (_data != null) _data.PropertyChanged += OnSubModelPropertyChanged;
            OnPropertyChanged();
        }
    }

    private int? _position { get; set; }
    public int? Position
    {
        get => _position;
        set
        {
            if (ReferenceEquals(_position, value)) return;
            _position = value;
            OnPropertyChanged();
        }
    }


    public bool Selected { get; set; }

    public AlertsModel Alerts { get; init; } = new ();
}

public enum ModLocation: byte
{
    Unknow = 0,
    DLC = 1,
    Local = 2,
    Steam = 4,
    MetaData = 8,
}
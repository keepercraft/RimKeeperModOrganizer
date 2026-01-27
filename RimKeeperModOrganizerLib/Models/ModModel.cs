using RimKeeperModOrganizerLib.Helpers;
using System.ComponentModel;

namespace RimKeeperModOrganizerLib.Models;

public class ModModel : INotifyPropertyChanged
{
    public string Label => About?.Name ?? About?.PackageId ?? "??";

    public string? Path { get; set; }
    public bool? Local { get; set; }
    public AboutModel? About { get; set; }
    public ModDataModel? Data { get; set; }

    public string? ThumbnailPath { get; set; }

    public int Position { get; set; }
    public bool Selected { get; set; }

    public List<string> Alert { get; } = new List<string>();
    public bool HasAlert => Alert.Any() == true;

    public string Versions => About?.SupportedVersions?.Order().Aggregate((a,b) => a+","+b) ?? "";

    public string SteamLink => string.Format(@"https://steamcommunity.com/sharedfiles/filedetails/?id={0}", About?.SteamId);

    public ModModel(string path, bool local = false)
    {
        Path = path;
        if (!Directory.Exists(Path)) return;
        About = XMLHelper.LoadAboutFromModPath(path);
        if (About == null) return;
        ThumbnailPath = FileHelper.GetModPreview(path);
        About.SteamId = FileHelper.GetModPublishID(path);
        Local = local;
    }

    public void TrySet(LocalDataListModel modelList)
    {
        if (string.IsNullOrEmpty(About?.PackageId)) return;
        foreach (ModDataModel item in modelList.ModDataList.Where(w => w.PackageId == About.PackageId))
        {
            Data = item;
            return;
        }
        Data = new ModDataModel() { PackageId = About.PackageId };
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

using KeeperBaseLib.Model;
using RimKeeperModOrganizerLib.Helpers;
namespace RimKeeperModOrganizerLib.Models;

public class ModModel : PropertyModel
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

    public string Versions => About?.SupportedVersions != null && About.SupportedVersions.Any()
        ? string.Join(",", About.SupportedVersions.OrderBy(v => v))
        : "";

    public string? SteamLink => String.IsNullOrEmpty(About?.SteamId) ? null : string.Format(@"https://steamcommunity.com/sharedfiles/filedetails/?id={0}", About?.SteamId);

    public ModModel() 
    {
    }
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
}

public static class ModModelExtension
{

}
using KeeperBaseLib.Model;
namespace RimKeeperModOrganizerLib.Models;

public class SettingsModel : PropertyModel
{
    public string PathSettings { get; set; } = "AppSettings.json";
    public string PathModData { get; set; } = "LoadLocalData.xml";
    public string PathDirGame { get; set; } = "";
    public string PathDirGameConfig { get; set; } = "";
    public string PathDirModsSteam { get; set; } = "";
    public string PathDirModsLocal { get; set; } = "";
    public string GameVersion { get; set; } = "";
}
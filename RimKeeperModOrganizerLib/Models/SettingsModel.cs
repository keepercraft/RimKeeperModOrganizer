using KeeperBaseLib.Model;
namespace RimKeeperModOrganizerLib.Models;

public class SettingsModel : PropertyModel
{
    public string PathSettings { get; set; } = "AppSettings.json";
    public string PathModData { get; set; } = "LoadLocalData.xml";
    public string PathDirSteam { get; set; } = "";
    public string PathDirGame { get; set; } = "";
    public string PathDirGameConfig { get; set; } = "";
    public string PathDirModsSteam { get; set; } = "";
    public string PathDirModsLocal { get; set; } = "";  
    public string GameVersion { get; set; } = "";

    public Dictionary<string, ColumnSettings> ModColumnData { get; set; } = new Dictionary<string, ColumnSettings>()
    {
        { "type", new ColumnSettings("S")  },
        { "group", new ColumnSettings("Grupa") },
        { "label", new ColumnSettings("Nazwa")  },
        { "author", new ColumnSettings("Autor") },
        { "packageId", new ColumnSettings("PackageId") },
        { "versions", new ColumnSettings("Varsion") }
    };
}

public class ColumnSettings : PropertyModel
{
    public ColumnSettings(string name) => Name = name;

    public string Name { get; set; } = string.Empty;       // unikalna nazwa kolumny
    public bool Visible { get; set; } = true;             // true = Visible, false = Collapsed
    public double Width { get; set; } = 100;
    public int DisplayIndex { get; set; } = 0;
}
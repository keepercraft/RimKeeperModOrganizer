using KeeperBaseLib.Model;
namespace RimKeeperModOrganizerLib.Models;

public class SettingsModel : PropertyModel
{
    public string PathSettings { get; set; } = "AppSettings.json";
    public string PathModData { get; set; } = "LoadLocalData.json";
    public string PathDirSteam { get; set; } = "";
    public string PathDirGame { get; set; } = "";
    public string PathDirGameConfig { get; set; } = "";
    public string PathDirModsSteam { get; set; } = "";
    public string PathDirModsLocal { get; set; } = "";  
    public string GameVersion { get; set; } = "";

    public WidowSettings MainWidow { get; set; } = new();

    public List<ColumnSettings> ModColumnData { get; set; } = new List<ColumnSettings>()
    {
        { new ColumnSettings("","type")  },
        { new ColumnSettings("Grupa","group") },
        { new ColumnSettings("Nazwa","label")  },
        { new ColumnSettings("Autor","author") },
        { new ColumnSettings("Packageid","packageid") },
        { new ColumnSettings("Varsion","versions") }
    };
}

public class ColumnSettings : PropertyModel
{
    public ColumnSettings() { }
    public ColumnSettings(string name, string key)
    {
        Name = name;
        Key = key;
    }

    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;       // unikalna nazwa kolumny
         
    private bool _visible = true; // true = Visible, false = Collapsed
    public bool Visible
    {
        get => _visible;
        set { _visible = value; OnPropertyChanged(); }
    }
    private string _width = "*";
    public string Width
    {
        get => _width;
        set { _width = value; OnPropertyChanged(); }
    }
    private int _displayIndex = 0; // true = Visible, false = Collapsed
    public int DisplayIndex
    {
        get => _displayIndex;
        set { _displayIndex = value; OnPropertyChanged(); }
    }
}

public class WidowSettings : PropertyModel
{
    public double Width { get; set; } = 500;
    public double Height { get; set; } = 500;
    public double Left { get; set; } = 0;
    public double Top { get; set; } = 0;
}
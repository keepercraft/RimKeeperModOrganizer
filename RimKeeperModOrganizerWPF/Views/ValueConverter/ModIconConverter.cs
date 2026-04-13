using RimKeeperModOrganizerLib.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
namespace RimKeeperModOrganizerWPF.Views.ValueConverter;
 
public class ModIconConverter : IValueConverter
{
    private ImageSource Get(string key) => (ImageSource)Application.Current.Resources[key];

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if(value == null) return null;
        ModLocation location = ModLocation.Unknow;
        if (value is ModLocation lod)
        {
            location = lod;
        }
        else if (value is string txt)
        {
            if (string.IsNullOrEmpty(txt)) return Get("DashIcon");
            Enum.TryParse(txt, out location);
        }
        else if (value is ModModel mod)
        {
            location = mod.Location;
        }
        switch (location)
        {
            case ModLocation.Local:
                return Get("FolderIcon");
            case ModLocation.Steam:
                return Get("SteamIcon");
            case ModLocation.DLC:
                return Get("GearIcon");
            case ModLocation.MetaData:
                return Get("MetaDataIcon");
            default:
                return Get("WarningIcon");
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
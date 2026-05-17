using RimKeeperModOrganizerLib.Models;
using System.Reflection;
using System.Text;
namespace RimKeeperModOrganizerLib.Extensions;

public static class AboutModelExtension
{
    public static void TryParsePackageId(this AboutModel model)
    {
        if (string.IsNullOrEmpty(model.PackageId))
        {
            string text = "none";
            if (!string.IsNullOrEmpty(model.Description))
            {
                text = StableStringHash(model.Description).ToString().Replace("-", "");
                text = text.Substring(0, Math.Min(3, text.Length));
            }
            string r = ConvertToASCII(model.Authors + text) + "." + ConvertToASCII(model.Name ?? "");
            model.PackageId = r.ToLower();
            model.PackageIdAutogen = true;
        }
    }

    public static void DetectPackageIdSteamSuffix(this IEnumerable<ModModel> list)
    {
        foreach (ModModel mod in list)
            if (mod.About != null)
                mod.About.DetectPackageIdSteamSuffix();
    }
    public static void DetectPackageIdSteamSuffix(this AboutModel model)
    {
        if (string.IsNullOrWhiteSpace(model.PackageId)) return;      
        if (model.PackageId.HasSteamSuffix())
        {
            model.PackageIdSteamSuffix = true;
            model.PackageId = model.PackageId.RemoveSteamSuffix();
        }
    }
    private const string _suffix = "_steam";
    public static bool HasSteamSuffix(this string packageId) => packageId.EndsWith(_suffix, StringComparison.OrdinalIgnoreCase);
    public static string RemoveSteamSuffix(this string packageId) => packageId[..^_suffix.Length];
    public static string ToSteamSuffix(this string packageId) => packageId+_suffix;

    public static int StableStringHash(string str)
    {
        if (str == null)
        {
            return 0;
        }
        int num = 23;
        int length = str.Length;
        for (int i = 0; i < length; i++)
        {
            num = num * 31 + (int)str[i];
        }
        return num;
    }
    public static string ConvertToASCII(string part)
    {
        StringBuilder stringBuilder = new StringBuilder("");
        foreach (char c in part)
        {
            char ch = c;
            if (!char.IsLetterOrDigit(ch) || ch >= '\u0080')
            {
                ch = (char)(ch % '\u0019' + 'A');
            }
            stringBuilder.Append(ch);
        }
        return stringBuilder.ToString();
    }
}

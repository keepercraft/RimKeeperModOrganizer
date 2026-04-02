using RimKeeperModOrganizerLib.Models;
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

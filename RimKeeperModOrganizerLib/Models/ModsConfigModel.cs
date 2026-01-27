namespace RimKeeperModOrganizerLib.Models;

public class ModsConfigModel
{
    public string? Version { get; set; }
    public List<string> ActiveMods { get; set; } = new();
    public List<string> KnownExpansions { get; set; } = new();


    public int Position(string? packageId)
    {
        if (string.IsNullOrEmpty(packageId)) return -1;
        return ActiveMods.IndexOf(packageId.ToLower());
    }
}

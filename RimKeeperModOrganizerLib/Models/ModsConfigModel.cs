namespace RimKeeperModOrganizerLib.Models;

public record ModsConfigModel
{
    public string? Version { get; set; }
    public List<string> ActiveMods { get; set; } = new();
    public List<string> KnownExpansions { get; set; } = new();

    public int? Position(string? packageId)
    {
        if (string.IsNullOrEmpty(packageId)) return null;
        int i = ActiveMods.IndexOf(packageId);
        return i < 0 ? null : i;
    }
}

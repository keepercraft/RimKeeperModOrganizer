namespace RimKeeperModOrganizerLib.Models;

public class AboutModel
{
    public string? Name { get; set; }
    public string? Author { get; set; }
    public string? SteamId { get; set; }
    public string? PackageId { get; set; }
    public List<string>? SupportedVersions { get; set; }
    public List<string>? LoadAfter { get; set; }
    public List<string>? LoadBefore { get; set; }
    public List<string>? IncompatibleWith { get; set; }
    public List<ModDependency>? ModDependencies { get; set; }
    public string? Description { get; set; }
}

public class ModDependency
{
    public string? PackageId { get; set; }
    public string? DisplayName { get; set; }
    public string? DownloadUrl { get; set; }
    public string? SteamWorkshopUrl { get; set; }
}

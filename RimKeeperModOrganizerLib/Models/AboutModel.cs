namespace RimKeeperModOrganizerLib.Models;

public record AboutModel
{
    public string? Name { get; set; }
    public string? Authors { get; set; }
    public string? SteamId { get; set; }
    public string? PackageId { get; set; }
    public List<string>? SupportedVersions { get; set; }
    public List<string>? LoadAfter { get; set; }
    public List<string>? LoadBefore { get; set; }
    public List<string>? IncompatibleWith { get; set; }
    public List<ModDependency>? ModDependencies { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public bool PackageIdAutogen { get; set; }
}

public record ModDependency
{
    public string? PackageId { get; set; }
    public string? DisplayName { get; set; }
    public string? DownloadUrl { get; set; }
    public string? SteamWorkshopUrl { get; set; }
}
 
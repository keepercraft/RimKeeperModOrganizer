using System.Text.Json;
using System.Text.Json.Serialization;
namespace RimKeeperModOrganizerLib.Helpers;

public static class SteamHelper
{
    private const string UrlGetWorkshopItems = "https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/";
    private const string UrlGetWorkshopSearch = "https://api.steampowered.com/IPublishedFileService/QueryFiles/v1/";
    private const string UrlGetWorkshopSub = "https://partner.steam-api.com/ISteamRemoteStorage/EnumerateUserSubscribedFiles/v1/";
    private const string Steam_dll = @"C:\Program Files (x86)\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Plugins\x86_64";
   // private const string dllPath = @"C:\Program Files (x86)\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Plugins\x86_64\steam_api64.dll";


    public static string? GetSteamId(string steamUrl)
    {
        var steamPath = Path.Combine(steamUrl, "config\\loginusers.vdf"); //@"C:\Program Files (x86)\Steam\config\loginusers.vdf";
        foreach (var line in File.ReadLines(steamPath))
            if (line.Trim().StartsWith("\"765"))
                return line.Trim().Split('"')[1];
        return null;
    }

    public static async Task<SteamWorkshopResponse?> GetWorkshopItemsAsync(List<string> modIds)
    {
        try
        {
            var values = new List<KeyValuePair<string, string>>
            {
                new("itemcount", modIds.Count.ToString())
            };
            for (int i = 0; i < modIds.Count; i++)
            {
                values.Add(new($"publishedfileids[{i}]", modIds[i]));
            }
            var content = new FormUrlEncodedContent(values);

            using HttpClient client = new();
            var response = await client.PostAsync(UrlGetWorkshopItems, content);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            var r = JsonSerializer.Deserialize<SteamWorkshopResponse>(json);
            return r;
        } catch { }
        return null;
    }
}

public class SteamWorkshopResponse
{
    [JsonPropertyName("response")]
    public WorkshopResponse Response { get; set; } = new();
}

public class WorkshopResponse
{
    [JsonPropertyName("result")]
    public int Result { get; set; }

    [JsonPropertyName("resultcount")]
    public int ResultCount { get; set; }

    [JsonPropertyName("publishedfiledetails")]
    public List<WorkshopMod> PublishedFileDetails { get; set; } = new();
}

public class WorkshopMod
{
    [JsonPropertyName("publishedfileid")]
    public string Id { get; set; } = "";

    [JsonPropertyName("result")]
    public int Result { get; set; }

    [JsonPropertyName("creator")]
    public string Creator { get; set; } = "";

    [JsonPropertyName("creator_app_id")]
    public int CreatorAppId { get; set; }

    [JsonPropertyName("consumer_app_id")]
    public int ConsumerAppId { get; set; }

    [JsonPropertyName("filename")]
    public string FileName { get; set; } = "";

    [JsonPropertyName("file_size")]
    public string FileSizeRaw { get; set; } = "";  // czasem string

    [JsonIgnore]
    public ulong FileSize => ulong.TryParse(FileSizeRaw, out var val) ? val : 0;

    [JsonPropertyName("file_url")]
    public string FileUrl { get; set; } = "";

    [JsonPropertyName("hcontent_file")]
    public string HContentFile { get; set; } = "";

    [JsonPropertyName("preview_url")]
    public string PreviewUrl { get; set; } = "";

    [JsonPropertyName("hcontent_preview")]
    public string HContentPreview { get; set; } = "";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("time_created")]
    public long TimeCreatedUnix { get; set; }

    [JsonIgnore]
    public DateTime Created => DateTimeOffset.FromUnixTimeSeconds(TimeCreatedUnix).UtcDateTime;

    [JsonPropertyName("time_updated")]
    public long TimeUpdatedUnix { get; set; }

    [JsonIgnore]
    public DateTime Updated => DateTimeOffset.FromUnixTimeSeconds(TimeUpdatedUnix).UtcDateTime;

    [JsonPropertyName("visibility")]
    public int Visibility { get; set; }

    [JsonPropertyName("banned")]
    public int BannedInt { get; set; }

    [JsonIgnore]
    public bool Banned => BannedInt != 0;

    [JsonPropertyName("ban_reason")]
    public string BanReason { get; set; } = "";

    [JsonPropertyName("subscriptions")]
    public int Subscriptions { get; set; }

    [JsonPropertyName("favorited")]
    public int Favorited { get; set; }

    [JsonPropertyName("lifetime_subscriptions")]
    public int LifetimeSubscriptions { get; set; }

    [JsonPropertyName("lifetime_favorited")]
    public int LifetimeFavorited { get; set; }

    [JsonPropertyName("views")]
    public int Views { get; set; }

    [JsonPropertyName("tags")]
    public List<Tag> Tags { get; set; } = new();

    [JsonIgnore]
    public List<string> TagNames => Tags?.ConvertAll(t => t.TagName) ?? new();
}

public class Tag
{
    [JsonPropertyName("tag")]
    public string TagName { get; set; } = "";
}
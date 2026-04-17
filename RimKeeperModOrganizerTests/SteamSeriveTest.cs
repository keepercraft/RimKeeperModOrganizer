using RimKeeperModOrganizerLib.Services;
namespace RimKeeperModOrganizerTests;

[TestFixture]
internal class SteamSeriveTest
{
    [Test]
    public void SteamServiceTest()
    {
        var modid = 3705023065;
        using SteamService steamService = new();
        RequestItemsDetailsAsync(steamService, modid);
    }
    public void RequestItemsDetailsAsync(SteamService steamService, ulong modid)
    {
        var data = steamService.RequestItemsDetailsAsync([modid]).Result;
    }

    [Test]
    public void SteamServiceSubTest()
    {
        var modid = 3705023065;
        using SteamService steamService = new();
        SubscribeItemAsync(steamService, modid);
    }
    public void SubscribeItemAsync(SteamService steamService, ulong modid)
    {
        var data = steamService.SubscribeItemAsync(modid).Result;
    }

    [Test]
    public void SteamServiceUnsubTest()
    {
        var modid = 3705023065;
        using SteamService steamService = new();
        UnsubscribeItemAsync(steamService, modid);
    }
    public void UnsubscribeItemAsync(SteamService steamService, ulong modid)
    {
        var data = steamService.UnsubscribeItemAsync(modid).Result;
    }
}
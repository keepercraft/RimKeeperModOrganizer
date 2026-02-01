using RimKeeperModOrganizerLib.Helpers;
using RimKeeperModOrganizerLib.Models;
using System.IO;

namespace RimKeeperModOrganizerTests;

[TestFixture]
internal class FileHelperTest
{
    [Test]
    public void FindSteamInstallPath()
    {
        string? path = FileHelper.FindSteamInstallPath();
        Assert.IsTrue(!string.IsNullOrEmpty(path));
    }

    [Test]
    public void FindRimWorldPath()
    {
        string? path = FileHelper.FindRimWorldGamePath();
        Assert.IsTrue(!string.IsNullOrEmpty(path));
    }

    [Test]
    public void FindRimWorldConfigPath()
    {
        string? path = FileHelper.FindRimWorldConfigPath();
        Assert.IsTrue(!string.IsNullOrEmpty(path));
    }

    [Test]
    public void FindRimWorldLocalMods()
    {
        string? path = FileHelper.FindRimWorldLocalModsPath();
        Assert.IsTrue(!string.IsNullOrEmpty(path));
    }

    [Test]
    public void FindRimWorldDLC()
    {
        string? path = FileHelper.FindRimWorldDLCPath();
        Assert.IsTrue(!string.IsNullOrEmpty(path));
    }

    [Test]
    public void FindRimWorldWorkshopMods()
    {
        var path = FileHelper.FindRimWorldWorkshopModsPaths();
        Assert.IsTrue(path.Any());
    }

    [Test]
    public void FindRimWorldAllMods()
    {
        var mods = FileHelper.FindRimWorldAllMods().ToList();
        Assert.IsTrue(mods.Any());
    }

    [Test]
    public void LoadMods()
    {
        var mods = FileHelper.GetRimWorldMods().ToList();
        Assert.IsTrue(mods.Any());
    }

    [Test]
    public void LoadModsConfig()
    {
        var mods = XMLHelper.LoadModsConfigFromConfigPath();
        Assert.IsTrue(mods != null);
    }
}

using KeeperBaseLib.Helper;
using RimKeeperModOrganizerLib.Models;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace RimKeeperModOrganizerLib.Helpers;

public static class XMLHelper
{
    public static AboutModel? LoadAboutFromModPath(string path)
    {
        var aboutPath = FileHelper.GetModAbout(path);
        if (aboutPath == null) return null;
        return LoadAbout(aboutPath);
    }
    public static AboutModel? LoadAbout(string aboutPath)
    {
        if (!File.Exists(aboutPath)) return null;
        try
        {
            var doc = XDocument.Load(aboutPath);
            var root = doc.Root;
            if (root == null)
                return null;

            var meta = new AboutModel
            {
                Name = root.Element("name")?.Value,
                Author = root.Element("author")?.Value,
                PackageId = root.Element("packageId")?.Value.ToLower(),
                Description = root.Element("description")?.Value,
                SupportedVersions = root.Element("supportedVersions")?
                                        .Elements("li")
                                        .Select(e => e.Value).ToList() ?? new List<string>(),
                LoadAfter = root.Element("loadAfter")?
                                        .Elements("li")
                                        .Select(e => e.Value).ToList() ?? new List<string>(),
                LoadBefore = root.Element("loadBefore")?
                                        .Elements("li")
                                        .Select(e => e.Value).ToList() ?? new List<string>(),
                IncompatibleWith = root.Element("incompatibleWith")?
                                        .Elements("li")
                                        .Select(e => e.Value).ToList() ?? new List<string>(),
                ModDependencies = root.Element("modDependencies")?
                                        .Elements("li")
                                        .Select(e => new ModDependency
                                        {
                                            PackageId = e.Element("packageId")?.Value.ToLower(),
                                            DisplayName = e.Element("displayName")?.Value,
                                            DownloadUrl = e.Element("downloadUrl")?.Value,
                                            SteamWorkshopUrl = e.Element("steamWorkshopUrl")?.Value,
                                        }).ToList() ?? new List<ModDependency>()
            };

            return meta;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd parsowania {aboutPath}: {ex.Message}");
            return null;
        }
    }

    public static ModsConfigModel? LoadModsConfigFromConfigPath()
    {
        var aboutPath = FileHelper.GetModsConfig();
        if (aboutPath == null) return null;
        return LoadModsConfig(aboutPath);
    }
    public static ModsConfigModel? LoadModsConfig(string configPath)
    {
        if (!File.Exists(configPath)) return null;
        try
        {
            var doc = XDocument.Load(configPath);
            var root = doc.Root;
            if (root == null) return null;

            var model = new ModsConfigModel
            {
                Version = root.Element("version")?.Value,
                ActiveMods = root.Element("activeMods")?
                                .Elements("li")
                                .Select(e => e.Value)
                                .ToList() ?? new List<string>(),
                KnownExpansions = root.Element("knownExpansions")?
                                .Elements("li")
                                .Select(e => e.Value)
                                .ToList() ?? new List<string>()
            };

            return model;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd parsowania ModsConfig.xml: {ex.Message}");
            return null;
        }
    }

    public static bool SaveModsConfig(string configPath, ModsConfigModel model)
    {
        if (model == null) return false;
        try
        {
            var doc = new XDocument(
                new XElement("ModsConfigData",
                    new XElement("version", model.Version ?? ""),
                    new XElement("activeMods",
                        model.ActiveMods.Select(m => new XElement("li", m))
                    ),
                    new XElement("knownExpansions",
                        model.KnownExpansions.Select(e => new XElement("li", e))
                    )
                )
            );

            doc.Save(configPath);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd zapisu ModsConfig.xml: {ex.Message}");
            return false;
        }
    }


    public static LocalDataListModel? LoadLocalData(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        try
        {
            var model = new LocalDataListModel();
            var doc = XDocument.Load(filePath);
            var root = doc.Root;
            doc.Root?
                .Elements("Mod")
                .Select(x =>
                {
                    var m = new ModDataModel
                    {
                        PackageId = (string?)x.Element("PackageId"),
                        Color = (string?)x.Element("Color"),
                        //Group = (string?)x.Element("Group"),
                    };
                    foreach (var s in x.Elements("Groups"))
                    {
                        m.Groups.Add(s.Value);
                    }
                    return m;
                })
                .Each(x => model.ModDataList.Add(x));
                //.ToList() ?? new List<ModDataModel>();
            return model;// new LocalDataListModel { ModDataList = modList };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd parsowania LoadLocalData: {ex.Message}");
            return null;
        }
    }

    public static void SaveLocalData(LocalDataListModel data, string filePath)
    {
        var doc = new XDocument(
            new XElement("Mods",
                data.ModDataList.Select(m =>
                    new XElement("Mod",
                        new XElement("PackageId", m.PackageId ?? string.Empty),                      
                        new XElement("Color", m.Color ?? string.Empty),
                        //new XElement("Group", m.Group ?? string.Empty),
                        new XElement("Groups", m.Groups.Select(i => new XElement("Group", i)))
                    )
                )
            )
        );
        doc.Save(filePath);
    }

}

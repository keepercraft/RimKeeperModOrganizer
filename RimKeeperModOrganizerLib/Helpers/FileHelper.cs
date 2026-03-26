using Microsoft.Win32;
using RimKeeperModOrganizerLib.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;
namespace RimKeeperModOrganizerLib.Helpers;

public static class FileHelper
{
    public static string? FindSteamInstallPath()
    {
        if (OperatingSystem.IsWindows())
        {
            using var key =
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam") ??
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam");

            return key?.GetValue("InstallPath") as string;
        }
        else if (OperatingSystem.IsLinux())
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string[] candidates =
            {
                Path.Combine(home, ".local", "share", "Steam"),
                Path.Combine(home, ".steam", "steam")
            };
            return candidates.FirstOrDefault(Directory.Exists);
        }
        else if (OperatingSystem.IsMacOS())
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string steamPath = Path.Combine(
                home,
                "Library",
                "Application Support",
                "Steam"
            );
            return Directory.Exists(steamPath) ? steamPath : null;
        }
        return null;
    }

    public static string? FindRimWorldGamePath(string? steamPath = null)
    {
        if (string.IsNullOrEmpty(steamPath)) steamPath = FindSteamInstallPath();
        if (string.IsNullOrEmpty(steamPath)) return null;
            //    Path.Combine(
            //    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            //    "Steam"
            //);

        string libraryFile = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(libraryFile)) return null;

        string content = File.ReadAllText(libraryFile);
        var matches = Regex.Matches(content, "\"path\"\\s*\"([^\"]+)\"");
        foreach (Match match in matches)
        {
            string libPath = match.Groups[1].Value.Replace(@"\\", @"\");
            string rimworld = Path.Combine(libPath, "steamapps", "common", "RimWorld");

            if (Directory.Exists(rimworld)) return rimworld;
        }
        return null;
    }

    public static string? FindRimWorldConfigPath()
    {
        if (OperatingSystem.IsWindows())
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Replace("Local", "LocalLow"),
                "Ludeon Studios",
                "RimWorld by Ludeon Studios",
                "Config"
            );
        }
        else if (OperatingSystem.IsLinux())
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config", "unity3d", "Ludeon Studios", "RimWorld by Ludeon Studios", "Config"
            );
        }
        else if (OperatingSystem.IsMacOS())
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Library", "Application Support", "RimWorld", "Config"
            );
        }
        else
        {
            return null;
        }
    }

    public static string? FindRimWorldLocalModsPath(string? location = null)
    {
        string? path = location ?? FindRimWorldGamePath();
        if (path == null) return null;
        return Path.Combine(path, "Mods");
    }

    public static string? FindRimWorldDLCPath(string? location = null)
    {
        string? path = location ?? FindRimWorldGamePath();
        if (path == null) return null;
        return Path.Combine(path, "Data");
    }

    public static string[] FindRimWorldWorkshopModsPaths(string? steamPath = null)
    {
        if (string.IsNullOrEmpty(steamPath)) steamPath = FindSteamInstallPath();
        if (string.IsNullOrEmpty(steamPath)) return null;

        string libraryFile = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(libraryFile)) return Array.Empty<string>();

        string content = File.ReadAllText(libraryFile);

        var matches = Regex.Matches(content, "\"path\"\\s*\"([^\"]+)\"");
        var results = new System.Collections.Generic.List<string>();

        foreach (Match match in matches)
        {
            string libPath = match.Groups[1].Value.Replace(@"\\", @"\");
            string workshopPath = Path.Combine(libPath, "steamapps", "workshop", "content", "294100");

            if (Directory.Exists(workshopPath)) results.Add(workshopPath);
        }
        return results.ToArray();
    }

    public static IEnumerable<ModModel> GetMods(string? path, bool? local = null)
    {
        if (Directory.Exists(path))
            foreach (var dir in Directory.GetDirectories(path))
            {
                var model = new ModModel(dir);
                if (model.About == null) continue;
                model.Local = local;
                yield return model;
            }      
    }
    public static IEnumerable<ModModel> GetRimWorldMods()
    {
        var workshopDirs = FindRimWorldWorkshopModsPaths();
        foreach (var workshopDir in workshopDirs)
        {
            if (!Directory.Exists(workshopDir)) continue;
            foreach (var dir in Directory.GetDirectories(workshopDir))
            {
                var model = new ModModel(dir);
                if (model.About == null) continue;
                yield return model;
            }
        }
    }


    public static IEnumerable<ModModel> FindRimWorldAllMods()
    {
        foreach (ModModel item in FileHelper.GetMods(FileHelper.FindRimWorldDLCPath(), null))
        {
            yield return item;
        }
        foreach (ModModel item in FileHelper.GetMods(FileHelper.FindRimWorldLocalModsPath(), true))
        {
            yield return item;
        }
        foreach (ModModel item in FileHelper.FindRimWorldWorkshopModsPaths().SelectMany(s => FileHelper.GetMods(s, false)))
        {
            yield return item;
        }
    }

    public static string? GetModAbout(string path)
    {
        string aboutFile = Path.Combine(path, "About", "About.xml");
        if (!File.Exists(aboutFile)) return null;
        return aboutFile;
    }
    public static string? GetModPublishID(string path)
    {
        string aboutFile = Path.Combine(path, "About", "PublishedFileId.txt");
        if (!File.Exists(aboutFile)) return null;
        return File.ReadLines(aboutFile).FirstOrDefault();
    }

    public static string? GetModPreview(string path)
    {
        string aboutFile = Path.Combine(path, "About", "Preview.png");
        if (!File.Exists(aboutFile)) return null;
        return aboutFile;
    }

    public static string? GetModsConfig(string? path = null)
    {
        if (string.IsNullOrEmpty(path)) path = FindRimWorldConfigPath();
        if (string.IsNullOrEmpty(path)) return null;
        string aboutFile = Path.Combine(path, "ModsConfig.xml");
        if (!File.Exists(aboutFile)) return null;
        return aboutFile;
    }

    public static string? GetRimworldVersion(string? path = null)
    {
        if (string.IsNullOrEmpty(path)) path = FindRimWorldGamePath();
        if (string.IsNullOrEmpty(path)) return null;
        string aboutFile = Path.Combine(path, "Version.txt");
        if (!File.Exists(aboutFile)) return null;
        return File.ReadLines(aboutFile).FirstOrDefault();
    }

    public static void OpenSteamLink(object? path)
    {
        if (path is string txt && !string.IsNullOrEmpty(txt))
            OpenLink($"steam://openurl/{txt}");
    }
    public static void OpenLink(object? path)
    {
        if(path is string txt && !string.IsNullOrEmpty(txt))
            Process.Start(new ProcessStartInfo
            {
                FileName = txt,
                UseShellExecute = true
            });
    }
}

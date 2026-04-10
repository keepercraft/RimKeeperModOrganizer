using RimKeeperModOrganizerLib.Helpers;
using RimKeeperModOrganizerLib.Models;
namespace RimKeeperModOrganizerLib.Extensions;

public static class AlertExtension
{
    public static void AlertUpdate(List<string> alerts, string alert, Func<bool> condition)
    {
        if (condition())
            if (!alerts.Contains(alert))
                alerts.Add(alert);
        else
            if (alerts.Contains(alert))
                alerts.Remove(alert);
    }

    public static void ModListAlertClean(this IEnumerable<ModModel> modlist)
    {
        foreach (var mod in modlist)
            mod.Alerts.Clear();
    }

    public static void ModListDuplicateValidation(this IEnumerable<ModModel> modlist)
    {
        foreach (var mod in modlist)
        {
            mod.Alerts.Clear([AlertType.MissingPath, AlertType.MissingPackageID, AlertType.DuplicatePackageID]);

            if (string.IsNullOrEmpty(mod.Path))
            {
                mod.Alerts.Add(AlertLevel.Warning, AlertType.MissingPath, mod.Label);
            }
            if (string.IsNullOrEmpty(mod.About?.PackageId))
            {
                mod.Alerts.Add(AlertLevel.Warning, AlertType.MissingPackageID, mod.Label);
            }
            if (modlist.Any(a => a != mod && a.About?.PackageId == mod.About?.PackageId))
            {
                mod.Alerts.Add(AlertLevel.Warning, AlertType.DuplicatePackageID, mod.Label);
            }
        }
    }

    public static void ModListValidation(this IEnumerable<ModModel> modlist, string? version = null)
    {
        int index = 0;
        string? short_version = ModHelper.ShortVersion(version);
        foreach (var mod in modlist)
        {
            mod.Alerts.Clear();

            if (string.IsNullOrEmpty(mod.Path))
            {
                mod.Alerts.Add(AlertLevel.Warning, AlertType.MissingPath, mod.Label);
            }

            if (!string.IsNullOrEmpty(short_version) && (mod.About?.SupportedVersions?.Any() ?? false))
            {
                if (!(mod.About.SupportedVersions.Any(c => c == short_version)))
                {
                    mod.Alerts.Add(AlertLevel.Critical, AlertType.Version, mod.Label);
                }
            }

            if (string.IsNullOrEmpty(mod.About?.PackageId)) continue;

            if (mod.About?.ModDependencies is { } mod_deps)
                foreach (var mod_dep in mod_deps)
                {
                    if (string.IsNullOrEmpty(mod_dep.PackageId)) continue;
                    if (!modlist.Any(a => a.About?.PackageId == mod_dep.PackageId))
                    {
                        mod.Alerts.Add(AlertLevel.Critical, AlertType.Dependency, mod_dep.PackageId);
                    }
                }

            if (mod.About?.IncompatibleWith is { } mod_incom_deps)
                foreach (var mod_dep_id in mod_incom_deps)
                {
                    if (string.IsNullOrEmpty(mod_dep_id)) continue;
                    if (modlist.Any(a => a.About?.PackageId == mod_dep_id))
                    {
                        mod.Alerts.Add(AlertLevel.Critical, AlertType.IncompatibleWith, mod_dep_id);
                    }
                }

            if (mod.About?.LoadAfter is { } mod_after_deps)
                foreach (var mod_dep_id in mod_after_deps)
                {
                    if (string.IsNullOrEmpty(mod_dep_id)) continue;
                    if (modlist.Skip(index + 1).Where(a => a.About?.PackageId == mod_dep_id).Any(a => a.About?.PackageId == mod_dep_id))
                    {
                        mod.Alerts.Add(AlertLevel.Critical, AlertType.LoadAfter, mod_dep_id);
                    }
                }

            if (mod.About?.LoadBefore is { } mod_before_deps)
                foreach (var mod_dep_id in mod_before_deps)
                {
                    if (string.IsNullOrEmpty(mod_dep_id)) continue;
                    if (modlist.Take(index).Where(a => a.About?.PackageId == mod_dep_id).Any(a => a.About?.PackageId == mod_dep_id))
                    {
                        mod.Alerts.Add(AlertLevel.Critical, AlertType.LoadBefore, mod_dep_id);
                    }
                }

            //mod.RaisePropertyChanged(nameof(ModModel.HasAlert));
            index++;
        }
    }
}

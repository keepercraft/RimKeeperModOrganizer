using RimKeeperModOrganizerLib.Models;
using RimKeeperModOrganizerLib.Services;
namespace RimKeeperModOrganizerLib.Extensions;

public static class SteamServiceExtension
{
    public static T? TryInitializeParse<T>(this SteamService service, ModModel? model, Func<SteamService, ulong, T> func)
    {
        if (model == null) return default!;
        string data;
        if (model.About != null && !string.IsNullOrEmpty(model.About.SteamId))
            data = model.About.SteamId;
        else
            data = model.GetSteamID();
        if (ulong.TryParse(data, out ulong pid_long))
        {
            return service.TryInitialize(c => func(c, pid_long));
        }
        return default!;
    }

    public static T? TryParse<T>(this SteamService service, ModModel? model, Func<SteamService, ulong, T> func)
    {
        if (model == null || model.About == null || string.IsNullOrEmpty(model.About.SteamId)) return default!;
        if (ulong.TryParse(model.About.SteamId, out ulong pid_long))
        {
            return func(service, pid_long);
        }
        return default!;
    }
}
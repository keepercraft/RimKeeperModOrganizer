using System;
using System.IO;
using System.Threading.Tasks;
namespace RimKeeperModOrganizerAvalonia.Helpers;

public static class TaskHelper
{
    public static async Task<bool> WaitFor(Func<bool> func, TimeSpan timeout)
    {
        var startTime = DateTime.Now;
        while (DateTime.Now - startTime < timeout)
        {
            if (func()) return true;
            await Task.Delay(500);
        }
        return false;
    }

    public static async Task<bool> WaitDirectoryExist(string path, int millisecondsDelay = 10000) 
        => await WaitFor(() => Directory.Exists(path), TimeSpan.FromSeconds(millisecondsDelay));

    public static async Task<bool> WaitDirectoryNotExist(string path, int millisecondsDelay = 10000)
        => await WaitFor(() =>
        {
            var t = !Directory.Exists(path);
                        return t;
        }, TimeSpan.FromSeconds(millisecondsDelay));
}
using System.Text.Json;
using System.Text.Json.Serialization;
namespace RimKeeperModOrganizerLib.Helpers;

public static class JsonHelper
{
    public static JsonSerializerOptions Options { get; } = new JsonSerializerOptions 
    { 
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static bool SerializeModel<T>(this T model, string path)
    {
        try
        {
            using FileStream fs = File.Create(path);
            JsonSerializer.Serialize(fs, model, Options);
            return true;
        }
        catch { }
        return false;
    }

    public static T? DeserializeModel<T>(string path)
    {
        try
        {
            if (!File.Exists(path)) return default;
            using FileStream fs = File.OpenRead(path);
            return JsonSerializer.Deserialize<T>(fs, Options);
        }
        catch { }
        return default;
    }

    public static async IAsyncEnumerable<T> DeserializeModelsAsync<T>(string path)
    {
        if (!File.Exists(path)) yield break;
        using FileStream fs = File.OpenRead(path);
        await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<T>(fs, Options))
        {
            if (item != null) yield return item;
        }
    }

    public static void DeserializeModel<T>(this T model, string path) where T : class, System.ComponentModel.INotifyPropertyChanged
    {
        //JsonSerializer.Populate(reader, model);
        T? temp = DeserializeModel<T>(path);
        if (temp == null) return;
        try
        {          
            foreach (var prop in typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (!prop.CanRead || !prop.CanWrite) continue;
                prop.SetValue(model, prop.GetValue(temp));
            }
        }
        catch { }
    }

    public static void LaodModsData()
    {
    }

    public static void SaveModsData()
    {
    }
}
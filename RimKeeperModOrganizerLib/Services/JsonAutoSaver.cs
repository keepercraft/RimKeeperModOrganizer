using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Timers;
using Timer = System.Timers.Timer;
namespace RimKeeperModOrganizerLib.Services;

public class JsonAutoSaver
{
    private string _lastHash = "";
    private readonly Timer _timer;
    private readonly Func<object> _getData;
    private readonly Action<string> _saveAction;
    private readonly JsonSerializerOptions? _jsonOptions;

    public JsonAutoSaver(
        Func<object> getData, 
        Action<string> saveAction,
        JsonSerializerOptions? jsonOptions = null,
        double intervalMs = 10000)
    {
        _jsonOptions = jsonOptions;
        _getData = getData ?? throw new ArgumentNullException(nameof(getData));
        _saveAction = saveAction ?? throw new ArgumentNullException(nameof(saveAction));

        _timer = new Timer(intervalMs);
        _timer.Elapsed += Timer_Elapsed;
        _timer.AutoReset = true;
        _timer.Start();
    }

    public void Calculate() => _lastHash = ComputeHash(_getData());
    public void Calculate(object? model) => _lastHash = ComputeHash(model);

    public void Trigger()
    {
        Timer_Elapsed();
        Restart();
    }
    public void Restart()
    {
        _timer.Stop();
        _timer.Start();
    }

    private void Timer_Elapsed(object? sender = null, ElapsedEventArgs? e = null)
    {
        if (string.IsNullOrEmpty(_lastHash)) return;
        try
        {
            var data = _getData();
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var hash = ComputeHashJson(json);
            if (hash != _lastHash)
            {
                _saveAction(json);
                _lastHash = hash;
            }
        }
        catch {}
    }

    private string ComputeHash(object? obj)
    {
        var json = JsonSerializer.Serialize(obj, _jsonOptions);
        return ComputeHashJson(json);
    }
    private string ComputeHashJson(string json)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(json);
        return Convert.ToBase64String(sha.ComputeHash(bytes));
    }
}
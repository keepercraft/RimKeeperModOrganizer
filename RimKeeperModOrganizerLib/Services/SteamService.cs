using Steamworks;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
namespace RimKeeperModOrganizerLib.Services;

public class SteamService : IDisposable
{
    public SteamServiceStatus LibraryStatus { get; private set; } = SteamServiceStatus.None;
    public bool IsInitialized { get; private set; } = false;
    public bool IsLibraryLoaded { get; private set; } = false;

    private CancellationTokenSource? _callbackTokenSource = null;
    private readonly object _steamLock = new object();
    private readonly List<object> _activeCallResults = new();
    private readonly SettingsService? _settings;

    public SteamService(SettingsService? settings = null)
    {
        _settings = settings;
        string path = settings?.Settings.PathDirGame;
        LoadLibrary(path);
        //Task.Run(CallbackLoop, _callbackTokenSource.Token);
    }

    public void Dispose()
    {
        DeInitialize();
    }

    public bool LoadLibrary(string? pathRimworld = null, string appId = "294100")
    {
        try
        {
            pathRimworld ??= _settings?.Settings.PathDirGame ?? @"C:\Program Files (x86)\Steam\steamapps\common\RimWorld";
            string pathApi = Path.Combine(pathRimworld, @"RimWorldWin64_Data\Plugins\x86_64", NativeMethods.SteamApiDllName);
            string pathDll = Path.Combine(pathRimworld, @"RimWorldWin64_Data\Managed", "com.rlabrecque.steamworks.net.dll");
            if (!File.Exists(pathApi) || !File.Exists(pathDll))
            {
                LibraryStatus = SteamServiceStatus.LibraryNotFound;
                return IsLibraryLoaded = false;
            }
            System.Runtime.InteropServices.NativeLibrary.Load(pathApi);
            System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(pathDll);
            Environment.SetEnvironmentVariable("SteamAppId", appId);
            Environment.SetEnvironmentVariable("SteamGameId", appId);
            LibraryStatus = SteamServiceStatus.None;
            return IsLibraryLoaded = true;
        }
        catch
        {
            LibraryStatus = SteamServiceStatus.LibraryLoadFailed;
            return IsLibraryLoaded = false;
        }
    }

    private bool TryConnect()
    {
        try
        {
            if (!SteamAPI.Init())
            {
                LibraryStatus = SteamServiceStatus.InitializationFailed;
                return false;
            }
            LibraryStatus = SteamServiceStatus.Initialize;
            return true;
        }
        catch
        {
            LibraryStatus = SteamServiceStatus.InternalError;
            return false;
        }
    }
    private bool EnsureConnected()
    {
        if(LibraryStatus == SteamServiceStatus.LibraryNotFound)
            if (_settings != null)
                LoadLibrary(_settings.Settings.PathDirGame);

        if (LibraryStatus != SteamServiceStatus.Initialize && LibraryStatus != SteamServiceStatus.None && LibraryStatus != SteamServiceStatus.InitializationFailed) return false;
        if (!SteamAPI.IsSteamRunning() || !IsInitialized)
        {
            IsInitialized = TryConnect();
        }
        return IsInitialized;
    }
    private async Task CallbackLoop()
    {
        try
        {
            while (!(_callbackTokenSource?.Token.IsCancellationRequested ?? true))
            {
                lock (_steamLock)
                {
                    if (IsInitialized) SteamAPI.RunCallbacks(); 
                    else break;
                }
                await Task.Delay(20, _callbackTokenSource.Token);
            }
        }
        catch (OperationCanceledException) { }
    }

    public bool Initialize(int millisecondsDelay = 5000)
    {
        bool connected = EnsureConnected();
        if (connected)
        {
            _callbackTokenSource = new CancellationTokenSource(millisecondsDelay);
            Task.Run(CallbackLoop, _callbackTokenSource.Token);
        }
        return connected;
    }
    public void DeInitialize()
    {
        lock (_steamLock)
        {
            if (!IsInitialized) return;
            _callbackTokenSource.Cancel();
            _callbackTokenSource.Dispose();
            _callbackTokenSource = null;
            lock (_activeCallResults) _activeCallResults.Clear();
            SteamAPI.Shutdown();
            LibraryStatus = SteamServiceStatus.None;
            IsInitialized = false;
        }
    }
    public T TryInitialize<T>(Func<SteamService, T> func, int millisecondsDelay = 10000)
    {
        T result = default!;
        bool initialize = false;
        try
        {
            initialize = Initialize(millisecondsDelay);
            result = func(this);
        }
        catch {    }
        finally
        {
            if(initialize) DeInitialize();
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task<T> ExecuteSteamCallAsync<T>(Func<SteamAPICall_t> steamAction)
    {
        if (!EnsureConnected()) throw new InvalidOperationException($"Steam not ready: {LibraryStatus}");
        var tcs = new TaskCompletionSource<T>();
        CallResult<T>? callResult = null;
        callResult = CallResult<T>.Create((res, failure) =>
        {
            lock (_activeCallResults) { if (callResult != null) _activeCallResults.Remove(callResult); }
            if (failure) tcs.TrySetException(new Exception($"Steam API IO Failure for {typeof(T).Name}"));
            else tcs.TrySetResult(res);
        });
        lock (_activeCallResults) { _activeCallResults.Add(callResult); }
        SteamAPICall_t handle = steamAction();
        callResult.Set(handle);
        return await tcs.Task.ConfigureAwait(false);
    }

    public bool IsSuccess<T>(T result, string fieldName = "m_eResult")
    {
        if (typeof(T).GetField(fieldName)?.GetValue(result) is EResult value)
            return value == EResult.k_EResultOK;
        return false;
    }

    public T GetResultSafe<T>(Task<T> task) => Task.Run(async () => await task.ConfigureAwait(false)).Result;


    public bool SubscribeItem(ulong modId) => IsSuccess(GetResultSafe(SubscribeItemAsync(modId)));
    public Task<RemoteStorageSubscribePublishedFileResult_t> SubscribeItemAsync(ulong modId)
        => ExecuteSteamCallAsync<RemoteStorageSubscribePublishedFileResult_t>(() => SteamUGC.SubscribeItem(new PublishedFileId_t(modId)));

    public bool UnsubscribeItem(ulong modId) => IsSuccess(GetResultSafe(UnsubscribeItemAsync(modId)));
    public Task<RemoteStorageUnsubscribePublishedFileResult_t> UnsubscribeItemAsync(ulong modId)
        => ExecuteSteamCallAsync<RemoteStorageUnsubscribePublishedFileResult_t>(() => SteamUGC.UnsubscribeItem(new PublishedFileId_t(modId)));

    public Task<SteamUGCRequestUGCDetailsResult_t> RequestItemsDetailsAsync(IEnumerable<ulong> modIds)
        => ExecuteSteamCallAsync<SteamUGCRequestUGCDetailsResult_t>(() =>
        {
            var publishIds = modIds.Select(id => new PublishedFileId_t(id)).ToArray();
            var queryHandle = SteamUGC.CreateQueryUGCDetailsRequest(publishIds, (uint)publishIds.Length);
            var apiCall = SteamUGC.SendQueryUGCRequest(queryHandle);
            SteamUGC.ReleaseQueryUGCRequest(queryHandle);
            return apiCall;
        });
}

public enum SteamServiceStatus
{
    None,
    Initialize,
    LibraryNotFound,
    LibraryLoadFailed,
    InitializationFailed,
    InternalError,
}

[SuppressUnmanagedCodeSecurity]
internal static class NativeMethods
{
    public const string SteamApiDllName = "steam_api64.dll";

    [DllImport(SteamApiDllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool SteamAPI_Init();

    [DllImport(SteamApiDllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SteamAPI_Shutdown();

    [DllImport(SteamApiDllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool SteamAPI_IsSteamRunning();
}
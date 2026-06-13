using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RimKeeperModOrganizerLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace RimKeeperModOrganizerAvalonia.Services;

//public class OpenDialog
//{
//    public static Window? GetMainWindow()
//    {
//        var desktop = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
//        return desktop?.MainWindow;
//    }
//    public static void ShowDialog<T>(object? dataContext = null) where T : Window, new()
//    {
//        Window? window = GetMainWindow();
//        if (window != null)
//        {
//            T new_window = new T();
//            if (dataContext != null)
//                new_window.DataContext = dataContext;
//            new_window.ShowDialog(window);
//        }
//    }

//    public static void ShowDialog<T, D>() 
//        where T : Window, new()
//        where D : new()
//    {
//        Window? window = GetMainWindow();
//        if (window != null)
//        {
//            T new_window = new T();
//            new_window.DataContext = new D();
//            new_window.ShowDialog(window);
//        }
//    }

//    public static void ShowServiceDialog<T>() where T : Window
//    {
//        Window? window = GetMainWindow();
//        if (window != null)
//        {
//            Program.Services.GetRequiredService<T>().Show(window);
//        }
//    }
//}

//public class OpenFileDialog
//{
//    public string? InitialDirectory { get; set; }
//    public string Title { get; set; } = "Open File";
//    public string Filter { get; set; } = "All files (*.*)|*.*";
//    public string? FileName { get; set; }
//    public string[] FileNames { get; private set; } = Array.Empty<string>();

//    public bool ShowDialog(Window owner)
//    {
//        var task = ShowDialogAsync(owner);
//        return Dispatcher.UIThread.InvokeAsync(async () => await task).Result ?? false;
//    }

//    public void ShowDialog<T>(IServiceProvider services, Action<bool, string?> onClosed) where T : Window
//    {
//        var window = Program.Services.GetRequiredService<T>();
//        if (window != null)
//            ShowDialog(window, onClosed);
//    }
//    public void ShowDialog(Action<bool, string?> onClosed)
//    {
//        var desktop = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
//        var window = desktop?.MainWindow;
//        if(window != null)
//            ShowDialog(window, onClosed);
//    }
//    public void ShowDialog(Window owner, Action<bool, string?> onClosed)
//    {
//        Task.Run(async () =>
//        {
//            var options = new FilePickerOpenOptions
//            {
//                Title = Title,
//                SuggestedFileName = FileName,
//                FileTypeFilter = ParseWpfFilter(Filter)
//            };

//            if (!string.IsNullOrEmpty(InitialDirectory))
//                options.SuggestedStartLocation = await owner.StorageProvider.TryGetFolderFromPathAsync(InitialDirectory);

//            // Wywołujemy okno na wątku UI
//            var result = await Dispatcher.UIThread.InvokeAsync(() => owner.StorageProvider.OpenFilePickerAsync(options));

//            // Wracamy do wątku UI z wynikiem
//            Dispatcher.UIThread.Post(() =>
//            {
//                if (result != null && result.Count > 0)
//                    onClosed(true, result[0].Path.LocalPath);
//                else
//                    onClosed(false, null);
//            });
//        });
//    }
//    public async Task<bool?> ShowDialogAsync(Window owner)
//    {
//        var options = new FilePickerOpenOptions
//        {
//            Title = Title,
//            AllowMultiple = false,
//            SuggestedFileName = FileName,
//            FileTypeFilter = ParseWpfFilter(Filter),
//        };

//        if (!string.IsNullOrEmpty(InitialDirectory))
//            options.SuggestedStartLocation = await owner.StorageProvider.TryGetFolderFromPathAsync(InitialDirectory);

//        var result = await owner.StorageProvider.OpenFilePickerAsync(options);

//        if (result != null && result.Count > 0)
//        {
//            FileNames = result.Select(r => r.Path.LocalPath).ToArray();
//            FileName = FileNames.FirstOrDefault();
//            return true;
//        }
//        return false;
//    }

//    private List<FilePickerFileType> ParseWpfFilter(string filter)
//    {
//        var result = new List<FilePickerFileType>();
//        var parts = filter.Split('|');
//        for (int i = 0; i < parts.Length; i += 2)
//        {
//            if (i + 1 < parts.Length)
//                result.Add(new FilePickerFileType(parts[i]) { Patterns = parts[i + 1].Split(';').Select(e => e.Trim()).ToArray() });
//        }
//        return result;
//    }
//}

//public class FolderDialogService
//{
//    public async void ShowDialog(string title, string? initialDir, Action<bool, string?> onClosed)
//    {
//        var desktop = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
//        var window = desktop?.MainWindow;
//        if (window != null)
//        {
//            string? result = await OpenFolderAsync(window, title, initialDir);
//            onClosed(result != null, result);
//        }
//    }
//    public async Task<string?> OpenFolderAsync(Window owner, string title, string? initialDir)
//    {
//        var options = new FolderPickerOpenOptions
//        {
//            Title = title
//        };

//        if (!string.IsNullOrEmpty(initialDir))
//        {
//            options.SuggestedStartLocation =
//                await owner.StorageProvider.TryGetFolderFromPathAsync(initialDir);
//        }

//        var result = await owner.StorageProvider.OpenFolderPickerAsync(options);

//        return result is { Count: > 0 }
//            ? result[0].Path.LocalPath
//            : null;
//    }
//}


//public class OpenFolderDialog2
//{
//    public OpenFolderDialog2(Window window)
//    {
//        var topLevel = TopLevel.GetTopLevel(window);
//        var options = new FolderPickerOpenOptions { Title = "test" };

//    }

//    public List<string> ShowDialog(Window window, string StartLocation) => AsyncShowDialog(window, StartLocation).GetAwaiter().GetResult();
//    public async Task<List<string>> AsyncShowDialog(Window window, string StartLocation)
//    {
//        var topLevel = TopLevel.GetTopLevel(window);
//        var options = new FolderPickerOpenOptions { Title = "test" };
//        options.SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(StartLocation);

//        var dialog = await topLevel.StorageProvider.OpenFolderPickerAsync(options);
//        var paths = dialog.Select(s => s.Path.LocalPath).ToList();
//        var counts = dialog.Count;
//        return paths;
//    }
//}


//public class OpenFolderDialog
//{
//    public string? InitialDirectory { get; set; }
//    public string Title { get; set; } = "Open Folder";
//    public string? FileName { get; set; }
//    public string[] FileNames { get; private set; } = Array.Empty<string>();

//    public bool ShowDialog(Window owner)
//    {
//        var task = ShowDialogAsync(owner);
//        return Dispatcher.UIThread.InvokeAsync(async () => await task).Result ?? false;
//    }

//    public void ShowDialog<T>(IServiceProvider services, Action<bool, string?> onClosed) where T : Window
//    {
//        var window = Program.Services.GetRequiredService<T>();
//        if (window != null)
//            ShowDialog(window, onClosed);
//    }
//    public void ShowDialog(Action<bool, string?> onClosed)
//    {
//        var desktop = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
//        var window = desktop?.MainWindow;
//        if (window != null)
//            ShowDialog(window, onClosed);
//    }
//    public async void ShowDialog(Window owner, Action<bool, string?> onClosed)
//    {
//        // Uruchamiamy zadanie w tle
//        //Task.Run(async () =>
//        //{
//            var options = new FolderPickerOpenOptions
//            {
//                Title = Title,
//                SuggestedFileName = FileName
//            };

//            if (!string.IsNullOrEmpty(InitialDirectory))
//                options.SuggestedStartLocation = await owner.StorageProvider.TryGetFolderFromPathAsync(InitialDirectory);

//            var model = await owner.StorageProvider.OpenFolderPickerAsync(options);


//            // Wywołujemy okno na wątku UI
//            //var result = await Dispatcher.UIThread.InvokeAsync(() => owner.StorageProvider.OpenFolderPickerAsync(options));

//        // Wracamy do wątku UI z wynikiem
//        //Dispatcher.UIThread.Post(() =>
//        //{
//        //    if (result != null && result.Count > 0)
//        //        onClosed(true, result[0].Path.LocalPath);
//        //    else
//        //        onClosed(false, null);
//        //});
//        //});
//    }   


//    public async Task<bool?> ShowDialogAsync(Window owner)
//    {
//        var options = new FolderPickerOpenOptions
//        {
//            Title = Title,
//            AllowMultiple = false,
//            SuggestedFileName = FileName
//        };

//        if (!string.IsNullOrEmpty(InitialDirectory))
//            options.SuggestedStartLocation = await owner.StorageProvider.TryGetFolderFromPathAsync(InitialDirectory);

//        var result = await owner.StorageProvider.OpenFolderPickerAsync(options);

//        if (result != null && result.Count > 0)
//        {
//            FileNames = result.Select(r => r.Path.LocalPath).ToArray();
//            FileName = FileNames.FirstOrDefault();
//            return true;
//        }
//        return false;
//    }

//    private List<FilePickerFileType> ParseWpfFilter(string filter)
//    {
//        var result = new List<FilePickerFileType>();
//        var parts = filter.Split('|');
//        for (int i = 0; i < parts.Length; i += 2)
//        {
//            if (i + 1 < parts.Length)
//                result.Add(new FilePickerFileType(parts[i]) { Patterns = parts[i + 1].Split(';').Select(e => e.Trim()).ToArray() });
//        }
//        return result;
//    }
//}

//public class SaveFileDialog
//{
//    public string? InitialDirectory { get; set; }
//    public string Title { get; set; } = "Save file";
//    public string Filter { get; set; } = "All files (*.*)|*.*";
//    public string? DefaultExt { get; set; }
//    public string? FileName { get; set; }
//    public string? ResultPath { get; private set; }


//    public void ShowDialog<T>(IServiceProvider services, Action<bool, string?> onClosed) where T : Window
//    {
//        var window = Program.Services.GetRequiredService<T>();
//        if (window != null)
//            ShowDialog(window, onClosed);
//    }
//    public void ShowDialog(Action<bool, string?> onClosed)
//    {
//        var desktop = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
//        var window = desktop?.MainWindow;
//        if (window != null)
//            ShowDialog(window, onClosed);
//    }
//    public void ShowDialog(Window owner, Action<bool, string?> onClosed)
//    {
//        // Uruchamiamy zadanie w tle, aby nie "zamrozić" wywołującego
//        Task.Run(async () =>
//        {
//            var options = new FilePickerSaveOptions
//            {
//                Title = Title,
//                SuggestedFileName = FileName,
//                DefaultExtension = DefaultExt,
//                FileTypeChoices = ParseWpfFilter(Filter),
//                ShowOverwritePrompt = true
//            };

//            if (!string.IsNullOrEmpty(InitialDirectory))
//            {
//                options.SuggestedStartLocation = await owner.StorageProvider.TryGetFolderFromPathAsync(InitialDirectory);
//            }

//            // Wywołujemy okno zapisu na wątku UI (wymagane przez Avalonię)
//            var result = await Dispatcher.UIThread.InvokeAsync(() => owner.StorageProvider.SaveFilePickerAsync(options));

//            // Wracamy do wątku UI z wynikiem, aby onClosed mógł bezpiecznie manipulować UI
//            Dispatcher.UIThread.Post(() =>
//            {
//                if (result != null)
//                {
//                    ResultPath = result.Path.LocalPath;
//                    onClosed(true, ResultPath);
//                }
//                else
//                {
//                    onClosed(false, null);
//                }
//            });
//        });
//    }
//    public async Task<bool> ShowDialogAsync(Window owner)
//    {
//        var options = new FilePickerSaveOptions
//        {
//            Title = Title,
//            SuggestedFileName = FileName,
//            DefaultExtension = DefaultExt,
//            FileTypeChoices = ParseWpfFilter(Filter),
//            ShowOverwritePrompt = true // Odpowiednik standardowego zachowania WPF
//        };

//        if (!string.IsNullOrEmpty(InitialDirectory))
//        {
//            options.SuggestedStartLocation = await owner.StorageProvider.TryGetFolderFromPathAsync(InitialDirectory);
//        }

//        var result = await owner.StorageProvider.SaveFilePickerAsync(options);

//        if (result != null)
//        {
//            ResultPath = result.Path.LocalPath;
//            return true;
//        }

//        return false;
//    }

//    private List<FilePickerFileType> ParseWpfFilter(string filter)
//    {
//        var result = new List<FilePickerFileType>();
//        var parts = filter.Split('|');

//        for (int i = 0; i < parts.Length; i += 2)
//        {
//            if (i + 1 < parts.Length)
//            {
//                result.Add(new FilePickerFileType(parts[i])
//                {
//                    Patterns = parts[i + 1].Split(';').Select(e => e.Trim()).ToArray()
//                });
//            }
//        }
//        return result;
//    }
//}
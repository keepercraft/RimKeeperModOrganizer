using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace RimKeeperModOrganizerAvalonia.Services;

public static class WindowLocator
{
    public static Window? MainWindow => (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

    public static TopLevel? GetTopLevel(Window? owner)
    {
        var targetWindow = owner ?? WindowLocator.MainWindow;
        if (targetWindow == null) return null;
        return TopLevel.GetTopLevel(targetWindow);
    }
}

public static class DialogService
{
    private static IServiceProvider? _services;
    public static void Init(IServiceProvider services)
    {
        _services = services;
    }

    public static Window? GetWindow<T>(object? dataContext = null) 
        where T : Window
    {
        var window = _services?.GetService<T>();
        if (window is null)
        {
            try
            {
                window = Activator.CreateInstance<T>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Cannot create window {typeof(T).Name}. Missing DI registration and no parameterless constructor.", ex);
            }
        }

        if (dataContext != null)
            window.DataContext = dataContext;

        return window;
    }

    public static Task ShowDialogWindow<T>(object? dataContext = null)
        where T : Window
        => WindowLocator.MainWindow?.ShowDialogWindow<T>(dataContext) ?? Task.CompletedTask;
    public static Task ShowDialogWindow<T>(this Window owner, object? dataContext = null)
        where T : Window
        => GetWindow<T>(dataContext)?.ShowDialog(owner) ?? Task.CompletedTask;

    public static void ShowWindow<T>(object? dataContext = null)
        where T : Window
        => WindowLocator.MainWindow?.ShowWindow<T>(dataContext);
    public static void ShowWindow<T>(this Window owner, object? dataContext = null)
        where T : Window
        => GetWindow<T>(dataContext)?.Show(owner);

    public static Task ShowDialogWindow<T, D>()
        where T : Window
        where D : class, new()
        => WindowLocator.MainWindow?.ShowDialogWindow<T, D>() ?? Task.CompletedTask;
    public static Task ShowDialogWindow<T, D>(this Window owner)
        where T : Window
        where D : class, new()
    {
        var dataContext = _services?.GetService<D>() ?? Activator.CreateInstance<D>();
        return GetWindow<T>(dataContext)?.ShowDialog(owner) ?? Task.CompletedTask;
    }


}

public static class StorageDialog
{
    public async static Task<List<string>> OpenFolderDialog(
        string title = "Open Folder",
        Window? owner = null, 
        string? startLocation = null,
        string? SuggestedFileName = null,
        bool AllowMultiple = false)
    {
        var topLevel = WindowLocator.GetTopLevel(owner);
        if (topLevel == null) return new List<string>();

        var options = new FolderPickerOpenOptions { Title = title, SuggestedFileName = SuggestedFileName, AllowMultiple = AllowMultiple };
        if (!string.IsNullOrEmpty(startLocation)) options.SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(startLocation);

        var dialog = await topLevel.StorageProvider.OpenFolderPickerAsync(options);
        return dialog.Select(s => s.Path.LocalPath).ToList();
    }

    public async static Task<List<string>> OpenFileDialog(
        string title = "Open File",
        string filters = "All files (*.*)|*.*",
        Window? owner = null,
        string? startLocation = null,
        string? SuggestedFileName = null,
        bool AllowMultiple = false)
    {
        var topLevel = WindowLocator.GetTopLevel(owner);
        if (topLevel == null) return new List<string>();

        var options = new FilePickerOpenOptions { Title = title, SuggestedFileName = SuggestedFileName, AllowMultiple = AllowMultiple, FileTypeFilter = ParseWpfFilter(filters) };
        if (!string.IsNullOrEmpty(startLocation)) options.SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(startLocation);

        var dialog = await topLevel.StorageProvider.OpenFilePickerAsync(options);
        return dialog.Select(s => s.Path.LocalPath).ToList();
    }

    public async static Task<string?> SaveFileDialog(
        string title = "Save File",
        string filters = "All files (*.*)|*.*",
        string defaultExtension = "*.*",
        Window? owner = null,
        string? startLocation = null,
        string? SuggestedFileName = null)
    {
        var topLevel = WindowLocator.GetTopLevel(owner);
        if (topLevel == null) return null;

        var options = new FilePickerSaveOptions { Title = title, SuggestedFileName = SuggestedFileName, DefaultExtension = defaultExtension, FileTypeChoices = ParseWpfFilter(filters) };
        if (!string.IsNullOrEmpty(startLocation)) options.SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(startLocation);

        var dialog = await topLevel.StorageProvider.SaveFilePickerAsync(options);
        return dialog?.Path.LocalPath;
    }


    private static List<FilePickerFileType> ParseWpfFilter(string filter)
    {
        var result = new List<FilePickerFileType>();
        var parts = filter.Split('|');
        for (int i = 0; i < parts.Length; i += 2)
        {
            if (i + 1 < parts.Length)
                result.Add(new FilePickerFileType(parts[i]) { Patterns = parts[i + 1].Split(';').Select(e => e.Trim()).ToArray() });
        }
        return result;
    }
}

public enum MessageBoxButton { OK, YesNo }
public enum MessageBoxResult { None, OK, Yes, No }

public static class MessageBox
{
    // Wersja asynchroniczna (Zalecana)
    public static async Task<MessageBoxResult> ShowAsync(string text, string title, MessageBoxButton buttons)
    {
        // Sprawdzamy, czy jesteśmy na wątku UI. Jeśli nie - "przeskakujemy" na niego.
        if (!Dispatcher.UIThread.CheckAccess())
        {
            return await Dispatcher.UIThread.InvokeAsync(() => ShowAsync(text, title, buttons));
        }

        var desktop = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var owner = desktop?.MainWindow;

        if (owner == null) return MessageBoxResult.None;

        // Okno tworzone bezpiecznie na wątku UI
        var dialog = new MessageBoxWindow(title, text, buttons);

        // ShowDialog blokuje interakcję z oknem pod spodem, ale nie mrozi aplikacji
        await dialog.ShowDialog(owner);

        return dialog.Result;
    }

    // Wersja z Callbackiem (Bez await w kodzie wywołującym)
    public static void Show(string text, string title, MessageBoxButton buttons, Action<MessageBoxResult> onClosed)
    {
        Task.Run(async () =>
        {
            var result = await ShowAsync(text, title, buttons);
            Avalonia.Threading.Dispatcher.UIThread.Post(() => onClosed(result));
        });
    }
}
internal class MessageBoxWindow : Window
{
    public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

    public MessageBoxWindow(string title, string text, MessageBoxButton buttons)
    {
        Title = title;
        SizeToContent = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        MinWidth = 300;
        MaxWidth = 500;
        Padding = new Thickness(20);
        Background = Brushes.White;

        var stackPanel = new StackPanel { Spacing = 20 };

        // Treść wiadomości
        stackPanel.Children.Add(new TextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        // Kontener na przyciski
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 10
        };

        if (buttons == MessageBoxButton.OK)
        {
            AddButton(buttonPanel, "OK", MessageBoxResult.OK, isDefault: true);
        }
        else if (buttons == MessageBoxButton.YesNo)
        {
            AddButton(buttonPanel, "Yes", MessageBoxResult.Yes, isDefault: true);
            AddButton(buttonPanel, "No", MessageBoxResult.No);
        }

        stackPanel.Children.Add(buttonPanel);
        Content = stackPanel;
    }

    private void AddButton(StackPanel panel, string text, MessageBoxResult result, bool isDefault = false)
    {
        var btn = new Button
        {
            Content = text,
            MinWidth = 80,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        btn.Click += (_, _) => { Result = result; Close(); };
        panel.Children.Add(btn);
    }
}
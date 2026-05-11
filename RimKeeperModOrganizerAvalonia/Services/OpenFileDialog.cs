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

public class OpenDialog
{
    public static Window? GetMainWindow()
    {
        var desktop = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        return desktop?.MainWindow;
    }
    public static void ShowDialog<T>(object? dataContext = null) where T : Window, new()
    {
        Window? window = GetMainWindow();
        if (window != null)
        {
            T new_window = new T();
            if (dataContext != null)
                new_window.DataContext = dataContext;
            new_window.ShowDialog(window);
        }
    }

    public static void ShowDialog<T, D>() 
        where T : Window, new()
        where D : new()
    {
        Window? window = GetMainWindow();
        if (window != null)
        {
            T new_window = new T();
            new_window.DataContext = new D();
            new_window.ShowDialog(window);
        }
    }

    public static void ShowServiceDialog<T>() where T : Window
    {
        Window? window = GetMainWindow();
        if (window != null)
        {
            Program.Services.GetRequiredService<T>().ShowDialog(window);
        }
    }
}

public class OpenFileDialog
{
    public string? InitialDirectory { get; set; }
    public string Title { get; set; } = "Open File";
    public string Filter { get; set; } = "All files (*.*)|*.*";
    public string? FileName { get; set; }
    public string[] FileNames { get; private set; } = Array.Empty<string>();

    // Metoda SYNCHRONICZNA (Styl WPF)
    public bool ShowDialog(Window owner)
    {
        // Uruchamiamy zadanie asynchroniczne i czekamy na wynik 
        // w sposób, który nie blokuje całkowicie pętli UI Avalonii
        var task = ShowDialogAsync(owner);

        // To jest "magiczna" linia, która pozwala uniknąć deadlocka w Avalonii,
        // ale wciąż zachowuje się synchronicznie dla wywołującego.
        return Dispatcher.UIThread.InvokeAsync(async () => await task).Result ?? false;
    }

    public void ShowDialog<T>(IServiceProvider services, Action<bool, string?> onClosed) where T : Window
    {
        var window = Program.Services.GetRequiredService<T>();
        if (window != null)
            ShowDialog(window, onClosed);
    }
    public void ShowDialog(Action<bool, string?> onClosed)
    {
        var desktop = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var window = desktop?.MainWindow;
        if(window != null)
            ShowDialog(window, onClosed);
    }
    public void ShowDialog(Window owner, Action<bool, string?> onClosed)
    {
        // Uruchamiamy zadanie w tle
        Task.Run(async () =>
        {
            var options = new FilePickerOpenOptions
            {
                Title = Title,
                SuggestedFileName = FileName,
                FileTypeFilter = ParseWpfFilter(Filter)
            };

            if (!string.IsNullOrEmpty(InitialDirectory))
                options.SuggestedStartLocation = await owner.StorageProvider.TryGetFolderFromPathAsync(InitialDirectory);

            // Wywołujemy okno na wątku UI
            var result = await Dispatcher.UIThread.InvokeAsync(() => owner.StorageProvider.OpenFilePickerAsync(options));

            // Wracamy do wątku UI z wynikiem
            Dispatcher.UIThread.Post(() =>
            {
                if (result != null && result.Count > 0)
                    onClosed(true, result[0].Path.LocalPath);
                else
                    onClosed(false, null);
            });
        });
    }
    // Metoda ASYNCHRONICZNA (Zalecana)
    public async Task<bool?> ShowDialogAsync(Window owner)
    {
        var options = new FilePickerOpenOptions
        {
            Title = Title,
            AllowMultiple = false,
            SuggestedFileName = FileName,
            FileTypeFilter = ParseWpfFilter(Filter),
        };

        if (!string.IsNullOrEmpty(InitialDirectory))
            options.SuggestedStartLocation = await owner.StorageProvider.TryGetFolderFromPathAsync(InitialDirectory);

        var result = await owner.StorageProvider.OpenFilePickerAsync(options);

        if (result != null && result.Count > 0)
        {
            FileNames = result.Select(r => r.Path.LocalPath).ToArray();
            FileName = FileNames.FirstOrDefault();
            return true;
        }
        return false;
    }

    private List<FilePickerFileType> ParseWpfFilter(string filter)
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

public class SaveFileDialog
{
    public string? InitialDirectory { get; set; }
    public string Title { get; set; } = "Save file";
    public string Filter { get; set; } = "All files (*.*)|*.*";
    public string? DefaultExt { get; set; }
    public string? FileName { get; set; }
    public string? ResultPath { get; private set; }


    public void ShowDialog<T>(IServiceProvider services, Action<bool, string?> onClosed) where T : Window
    {
        var window = Program.Services.GetRequiredService<T>();
        if (window != null)
            ShowDialog(window, onClosed);
    }
    public void ShowDialog(Action<bool, string?> onClosed)
    {
        var desktop = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var window = desktop?.MainWindow;
        if (window != null)
            ShowDialog(window, onClosed);
    }
    public void ShowDialog(Window owner, Action<bool, string?> onClosed)
    {
        // Uruchamiamy zadanie w tle, aby nie "zamrozić" wywołującego
        Task.Run(async () =>
        {
            var options = new FilePickerSaveOptions
            {
                Title = Title,
                SuggestedFileName = FileName,
                DefaultExtension = DefaultExt,
                FileTypeChoices = ParseWpfFilter(Filter),
                ShowOverwritePrompt = true
            };

            if (!string.IsNullOrEmpty(InitialDirectory))
            {
                options.SuggestedStartLocation = await owner.StorageProvider.TryGetFolderFromPathAsync(InitialDirectory);
            }

            // Wywołujemy okno zapisu na wątku UI (wymagane przez Avalonię)
            var result = await Dispatcher.UIThread.InvokeAsync(() => owner.StorageProvider.SaveFilePickerAsync(options));

            // Wracamy do wątku UI z wynikiem, aby onClosed mógł bezpiecznie manipulować UI
            Dispatcher.UIThread.Post(() =>
            {
                if (result != null)
                {
                    ResultPath = result.Path.LocalPath;
                    onClosed(true, ResultPath);
                }
                else
                {
                    onClosed(false, null);
                }
            });
        });
    }
    public async Task<bool> ShowDialogAsync(Window owner)
    {
        var options = new FilePickerSaveOptions
        {
            Title = Title,
            SuggestedFileName = FileName,
            DefaultExtension = DefaultExt,
            FileTypeChoices = ParseWpfFilter(Filter),
            ShowOverwritePrompt = true // Odpowiednik standardowego zachowania WPF
        };

        if (!string.IsNullOrEmpty(InitialDirectory))
        {
            options.SuggestedStartLocation = await owner.StorageProvider.TryGetFolderFromPathAsync(InitialDirectory);
        }

        var result = await owner.StorageProvider.SaveFilePickerAsync(options);

        if (result != null)
        {
            ResultPath = result.Path.LocalPath;
            return true;
        }

        return false;
    }

    private List<FilePickerFileType> ParseWpfFilter(string filter)
    {
        var result = new List<FilePickerFileType>();
        var parts = filter.Split('|');

        for (int i = 0; i < parts.Length; i += 2)
        {
            if (i + 1 < parts.Length)
            {
                result.Add(new FilePickerFileType(parts[i])
                {
                    Patterns = parts[i + 1].Split(';').Select(e => e.Trim()).ToArray()
                });
            }
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

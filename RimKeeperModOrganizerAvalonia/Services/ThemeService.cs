using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using RimKeeperModOrganizerLib.Models;
using RimKeeperModOrganizerLib.Services;
namespace RimKeeperModOrganizerAvalonia.Services;

public class ThemeService
{
    private readonly SettingsService _settingsService;
    private Application _application => Application.Current!;
    private SettingsModel _settingsModel => _settingsService.Settings;

    public bool ReloadMainwindow { get; set; } = true;

    public ThemeService(SettingsService SettingsService)    
    {
        _settingsService = SettingsService;
        _application.ActualThemeVariantChanged += OnThemeChanged;
        _settingsService.Settings.PropertyChanged += Settings_PropertyChanged;
        SetTheme(_settingsModel.WindowTheme);
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_settingsModel.WindowTheme))
        {
            ThemeFlag theme = _application.ActualThemeVariant == ThemeVariant.Dark
                ? ThemeFlag.Dark
                : ThemeFlag.Light;
            if (_settingsModel.WindowTheme != theme)
            {
                SetTheme(_settingsModel.WindowTheme);
                _settingsService.Save();
            }
        }
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        ThemeFlag theme = _application.ActualThemeVariant == ThemeVariant.Dark
            ? ThemeFlag.Dark
            : ThemeFlag.Light;
        if (_settingsModel.WindowTheme != theme)
        {
            _settingsModel.WindowTheme = theme;
            _settingsModel.RaisePropertyChanged(nameof(_settingsModel.WindowTheme));
            _settingsService.Save();
        }
        if (ReloadMainwindow) ReloadMainWindow();
    }

    public void SetTheme(ThemeVariant theme) => _application.RequestedThemeVariant = theme;
    public void SetTheme(ThemeFlag theme) => SetTheme(theme == ThemeFlag.Dark
        ? ThemeVariant.Dark
        : ThemeVariant.Light);
    public void SwitchTheme() => SetTheme(_application.ActualThemeVariant == ThemeVariant.Light
        ? ThemeVariant.Dark
        : ThemeVariant.Light);

    public void ReloadMainWindow()
    {
        var desktop = _application.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var window = desktop?.MainWindow;
        if (window == null) return;
        var content = window.Content;
        window.Content = null;
        window.Content = content;
        window.InvalidateVisual();
    }
}
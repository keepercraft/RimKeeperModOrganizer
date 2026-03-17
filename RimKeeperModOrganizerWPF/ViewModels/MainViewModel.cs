using KeeperBaseLib.Model;
using KeeperBaseWPFLib.MVVM;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using RimKeeperModOrganizerLib.Extensions;
using RimKeeperModOrganizerLib.Helpers;
using RimKeeperModOrganizerLib.Models;
using RimKeeperModOrganizerLib.Services;
using RimKeeperModOrganizerWPF.Extensions;
using RimKeeperModOrganizerWPF.Views;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Media;
namespace RimKeeperModOrganizerWPF.ViewModels;

public class MainViewModel : PropertyModel
{
    public ObservableCollection<string> ModGroups { get; set; } = new();
    public ObservableCollection<string> ModColors { get; set; } = new();
    public ObservableCollection<ModModel> ModsConfigCollection { get; } = new();
    public ObservableCollection<ModModel> ModsCollection { get; } = new();
    public Dictionary<string, ColumnSettings> ModColumnData => _settingsService.Settings.ModColumnData;
    public WidowSettings MainWidowSettings => _settingsService.Settings.MainWidow;

    private ModModel? _selectedMod;
    public ModModel? SelectedMod
    {
        get => _selectedMod;
        set
        {
            if (_selectedMod != value)
            {
                _selectedMod = value;
                RaisePropertyChanged(nameof(SelectedMod));
            }
        }
    }

    private readonly ModsServices _modsServices;
    private readonly SettingsService _settingsService;
    public MainViewModel(ModsServices modsServices, SettingsService SettingsService)
    {
        _modsServices = modsServices;
        _settingsService = SettingsService;
        ModsConfigCollection.CollectionChanged += ModsConfigCollection_CollectionChanged;
    }

    #region UI Locker 
    private bool _loadingUI = false;
    public bool LoadingUI
    {
        get => _loadingUI;
        set
        {
            if (_loadingUI != value)
            {
                _loadingUI = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsUIListEnabled));
            }
        }
    }
    public bool IsUIListEnabled => !LoadingUI;
    public void UILock(Action action)
    {
        try
        {
            LoadingUI = true;
            action();
        }
        catch { }
        finally { LoadingUI = false; }
    }
    #endregion

    #region Alert Section
    public bool IsModsConfigAlert => ModsConfigCollection.Union(ModsCollection).Where(x => x.Alert != null).Any(x => x.Alert.Any());
    public List<string> ListModsConfigAlert => ModsConfigCollection.Union(ModsCollection).Where(x => x.Alert != null).SelectMany(x => x.Alert).ToList();
    public Brush? GetModConfigStaticColor => IsModsConfigAlert ? Brushes.LightCoral : Brushes.Transparent;
    public string GetModConfigStaticLable => string.Format("Mods ({0}/{1})", ModsConfigCollection.Count, ModsCollection.Count);
    public void AlertPropertyChanged()
    {
        RaisePropertyChanged(nameof(ListModsConfigAlert));
        RaisePropertyChanged(nameof(GetModConfigStaticColor));
        RaisePropertyChanged(nameof(GetModConfigStaticLable));
    }
    #endregion

    public void LoadMods(string? path = null)
    {
        Task.Run(() =>
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                LoadingUI = true;
                RaisePropertyChanged(nameof(IsUIListEnabled));
                // ModsConfigCollection.Clear();
                ModsConfigCollection.Clear();
                ModsCollection.Clear();
                ModsConfigCollection.CollectionChanged -= ModsConfigCollection_CollectionChanged;
                AlertPropertyChanged();
            });
            foreach (var item in _modsServices.LoadMods2(path))
            {
                if (item != null)
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        //AllMods.Add(item);
                        if (item.Selected)
                        {
                            ModsConfigCollection.Add(item);
                            AlertPropertyChanged();
                        }
                        else
                            ModsCollection.Add(item);
                    });
            }
            App.Current.Dispatcher.Invoke(() =>
            {
                ModsConfigCollection.SortBy(s => s.Position, s => s.Position >= 0);
                ModsConfigCollection.ModListValidation();
                LoadData(ModsConfigCollection.Union(ModsCollection));
                ModsConfigCollection.CollectionChanged += ModsConfigCollection_CollectionChanged;
                LoadingUI = false;
                RaisePropertyChanged(nameof(IsUIListEnabled));
                AlertPropertyChanged();
            });
        });
    }
    public void LoadData(IEnumerable<ModModel> modlist)
    {
        var data = modlist.Select(s => s.Data).Where(s => s != null);
        foreach (var item in data.SelectMany(s => s.Groups).Where(w => !string.IsNullOrEmpty(w)).Distinct())
            if (!ModGroups.Contains(item))
                ModGroups.Add(item);
        foreach (var item in data.Select(s => s.Color).Where(w => !string.IsNullOrEmpty(w)).Distinct())
            if (!ModColors.Contains(item))
                ModColors.Add(item);
    }

    private void ModsConfigCollection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is ObservableCollection<ModModel> collection && collection == ModsConfigCollection)
        {
            if (e.NewItems?[0] is ModModel new_item_model)
            {
                new_item_model.Selected = true;
                new_item_model.Position = e.NewStartingIndex;
            }
            if (e.OldItems?[0] is ModModel old_item_model)
            {
                old_item_model.Selected = false;
                old_item_model.Position = -1;
            }
            for (int i = 0; i < collection.Count; i++)
            {
                collection[i].Position = i;
                collection[i].RaisePropertyChanged(nameof(ModModel.Position));
            }
            ModsConfigCollection.ModListValidation();
            AlertPropertyChanged();
            //ModListValidation();
        }
    }

    #region CustomCommand
    public CustomCommand OpenSteamLinkCommand => new CustomCommand(p =>
    {
        if (p is string txt) 
            OpenLinkCommand.Execute($"steam://openurl/{txt}");
    });
    public CustomCommand OpenLinkCommand => new CustomCommand(p =>
    {
        if(p is string txt)
            Process.Start(new ProcessStartInfo
            {
                FileName = txt,
                UseShellExecute = true
            });
    });
    public CustomCommand OptionsCommand => new CustomCommand(p => UILock(() =>
    {
        App.Services.GetRequiredService<SettingsWindow>().ShowDialog();
        RaisePropertyChanged(nameof(ModColumnData));
    }));
    public CustomCommand ChangeColorCommand => new CustomCommand(p => UILock(() =>
    {
        if (p != null && p is ModModel model) SelectedMod = model;
        if (SelectedMod == null) return;
        ModColors.Clear();
        foreach (var item in ModsConfigCollection.Union(ModsCollection)
            .Where(x => x.Data != null).Where(x => x.Data.IsNotNull())
            .Select(s => s.Data.Color).Where(w => !string.IsNullOrEmpty(w)).Distinct())
        {
            ModColors.Add(item);
        }
        new ChangeColorWindow(this).ShowDialog();
    }));
    public CustomCommand RefreshCommand => new CustomCommand(p => LoadMods());
    public CustomCommand LoadConfigCommand => new CustomCommand(p => UILock(() =>
    {
        var dialog = new OpenFileDialog
        {
            Title = "Wybierz plik",
            Filter = "XML (*.xml)|*.xml|Wszystkie pliki (*.*)|*.*",
            Multiselect = false
        };
        if (dialog.ShowDialog() ?? false) LoadMods(dialog.FileName);
    }));
    public CustomCommand SaveConfigCommand => new CustomCommand(p => UILock(() =>
    {
        var dialog = new SaveFileDialog
        {
            Title = "Zapisz plik",
            Filter = "XML (*.xml)|*.xml",
            DefaultExt = ".xml",
            FileName = "ModsConfig.xml"
        };
        if (dialog.ShowDialog() ?? false) _modsServices.SaveConfig(ModsConfigCollection, dialog.FileName);
    }));
    public CustomCommand SaveCommand => new CustomCommand(p => UILock(() =>
    {
        if (ModsConfigCollection.Union(ModsCollection).Any())
        {
            _modsServices.SaveConfig(ModsConfigCollection);
            _modsServices.SaveLocalData(ModsConfigCollection.Union(ModsCollection));
        }
    }));

    public CustomCommand RimpyColorLoadCommand => new CustomCommand(p => UILock(() =>
    {
        var dialog = new OpenFileDialog
        {
            Title = "Open file",
            Filter = "ini |*.ini",
            DefaultExt = ".ini",
            FileName = "config.ini"
        };
        if (dialog.ShowDialog() ?? false)
        { 
            var data = _modsServices.LoadRimPyColors(dialog.FileName);
            foreach(var item in ModsConfigCollection.Union(ModsCollection))
                if(item.Path != null && item.Data !=null && data.ContainsKey(item.Path))
                    item.Data.Color = data[item.Path];
        }
    }));
    public CustomCommand ModsToCSVCommand => new CustomCommand(p => UILock(() =>
    {
        var dialog = new SaveFileDialog
        {
            Title = "Zapisz plik",
            Filter = "CSV (*.csv)|*.csv",
            DefaultExt = ".xml",
            FileName = "Mods.csv"
        };
        if (dialog.ShowDialog() ?? false) _modsServices.ExportCSVMods(ModsConfigCollection.Union(ModsCollection), dialog.FileName);
    }));

    public CustomCommand ModDetailCommand => new CustomCommand(p => UILock(() => new ModDetailWindow(this).ShowDialog()));
    public CustomCommand ModDataRemoveCommand => new CustomCommand(p => UILock(() =>
    {
        if (p is ModModel model)
        {
            model.Data?.Clear();
            if (String.IsNullOrEmpty(model.Path))
            {
                ModsCollection.Remove(model);
                ModsConfigCollection.Remove(model);
            }
            AlertPropertyChanged();
        }
    }));

    public CustomCommand TestCommand => new CustomCommand(p => UILock(async () =>
    {
        var t = ModsConfigCollection.Union(ModsCollection)
            .Select(s => s.About?.SteamId)
            .Where(w => !string.IsNullOrEmpty(w))
            .Take(5)
            .ToList();

       //SteamHelper.GetModDetails(t[0]);
        //SteamHelper.GetWorkshopSearchAsync();
        // string? steamid = SteamHelper.GetSteamId(_settingsService.Settings.PathDirSteam);
        //await SteamHelper.GetSubscribedFilesAsync(steamid);
        /*
                var data = await SteamHelper.GetWorkshopItemsAsync(t);
                var data_f = data.Response.PublishedFileDetails.Select(s => new 
                { 
                    s.Id,
                    s.Updated,
                    s.Created,
                    s.TimeUpdatedUnix,
                    s.TimeCreatedUnix,
                    s.Views,
                    s.Favorited,
                    s.Subscriptions,
                }).ToList();
        */
    }));
    #endregion CustomCommand
}
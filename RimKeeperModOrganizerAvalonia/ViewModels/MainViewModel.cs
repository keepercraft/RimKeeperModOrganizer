using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using KeeperBaseSharedLib.Models;
using KeeperBaseSheredLib;
using KeeperDataGridAvalonia.Extensions;
using KeeperDataGridAvalonia.Models;
using RimKeeperModOrganizerAvalonia.Extensions;
using RimKeeperModOrganizerAvalonia.Helpers;
using RimKeeperModOrganizerAvalonia.Services;
using RimKeeperModOrganizerAvalonia.Views;
using RimKeeperModOrganizerLib.Extensions;
using RimKeeperModOrganizerLib.Helpers;
using RimKeeperModOrganizerLib.Models;
using RimKeeperModOrganizerLib.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
namespace RimKeeperModOrganizerAvalonia.ViewModels;

public class MainViewModel : PropertyModel
{
    public ObservableCollection<string> ModGroups { get; set; } = new();
    public ObservableCollection<string> ModColors { get; set; } = new();
    public ObservableCollection<IColumnConfig> ModsCollectionColumns { get; set; } = new();
    public ObservableCollection<ModModel> Items { get; set; } = new();
    public IDataGridCollectionView ModsConfigCollection { get; }
    public IDataGridCollectionView ModsCollection { get; }
    private ModModel? _selectedMod;
    public ModModel? SelectedMod
    {
        get => _selectedMod;
        set
        {
            _selectedMod = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsSelectedMod));
        }
    }
    public bool IsSelectedMod => SelectedMod != null;
    public List<ColumnSettings> ModColumnData => _settingsService.Settings.ModColumnData;
    public MainWidowSettings MainWidowSettings => _settingsService.Settings.MainWidow;
    public IEnumerable<string> ModTypeIconsList { get; set; }
    public bool SteamServiceReady => _steamService?.IsLibraryLoaded ?? false;

    private readonly JsonAutoSaver _autoSaver;
    private readonly ModsServices _modsServices;
    private readonly SettingsService _settingsService;
    private readonly SteamService _steamService;
    private readonly ThemeService _themeService;

    public string GameVersion => _settingsService.Settings.GameVersion;
    public PixelPoint WindowPosition
    {
        get => new PixelPoint((int)MainWidowSettings.Left, (int)MainWidowSettings.Top);
        set
        {
            MainWidowSettings.Left = value.X;
            MainWidowSettings.Top = value.Y;
        }
    }

    public IList<string>? SelectedModPreviews => FileHelper.GetModPreviews(SelectedMod?.Path??"");
    public string? _selectedModPreviewsSelection;
    public string? SelectedModPreviewsSelection
    {
        get => SelectedModPreviews != null 
            && _selectedModPreviewsSelection != null 
            && SelectedModPreviews.Contains(_selectedModPreviewsSelection) 
            ? _selectedModPreviewsSelection
            : SelectedMod?.ThumbnailPath?.ToLower();
        set
        {
            if (value == null) return;
            _selectedModPreviewsSelection = value;
            OnPropertyChanged();
        }
    }

    public string SteamIconKey { get; set; } = "Steam";

    public MainViewModel(SettingsService SettingsService, ModsServices modsServices, SteamService steamService, ThemeService themeService)
    {
        _settingsService = SettingsService;
        _modsServices = modsServices;
        _steamService = steamService;
        _themeService = themeService;
        _autoSaver = new JsonAutoSaver(
            () => DataChanged,
            js => _modsServices.SaveLocalData(Items),
            JsonHelper.Options,
            true);
        _autoSaver.Calculate(false);

        ModColumnData.BindToSettings(ModsCollectionColumns);
        ModsCollection = new DataGridCollectionView(Items); //CollectionViewSource.GetDefaultView(Items);
        ModsCollection.CombineFilters(LeftViewFilter);
        ModsConfigCollection = new DataGridCollectionView(Items); //CollectionViewSource.GetDefaultView(Items);
        ModsConfigCollection.CombineFilters(RightViewFilter);
        Items.CollectionChanged += Items_CollectionChanged;

        ModsCollection.CollectionChanged += (s, e) => RaisePropertyChanged(nameof(GetModListStaticLable));
        ModsConfigCollection.CollectionChanged += (s, e) => RaisePropertyChanged(nameof(GetModConfigStaticLable));

        ModTypeIconsList = new List<string>()
        {
            string.Empty,
            ModLocation.DLC.ToString(),
            ModLocation.Local.ToString(),
            ModLocation.Steam.ToString(),
            ModLocation.MetaData.ToString(),
        };
    }

    private bool LeftViewFilter(object obj) => ((ModModel)obj)?.Position == null;
    private bool RightViewFilter(object obj) => ((ModModel)obj)?.Position >= 0;

    public bool DataChanged = false;
    private void Data_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        => DataChanged = true;
    private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => e.SyncProperties<ModModel>(Data_PropertyChanged, c => c?.Data);


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
    public bool IsModsConfigAlert => ModsConfigCollection.Cast<ModModel>().Any(x => x.Alerts.HasAlert);
    public List<AlertModel> ListModsConfigAlerts => ModsConfigCollection.Cast<ModModel>().SelectMany(x => x.Alerts.Items).ToList();
    public IImmutableSolidColorBrush GetModConfigStaticColor => IsModsConfigAlert ? Brushes.LightCoral : Brushes.Transparent;

    public string GetModListStaticLable
    {
        get
        {
            return string.Format("Mods ({0}/{1})"
                , ModsCollection.Cast<object>().Count()
                , Items.Count()
             );
        }
    }
    public string GetModConfigStaticLable
    {
        get
        {
            var alerts = ModsConfigCollection.Cast<ModModel>().Sum(c => c.Alerts.Items.Count);
            return string.Format("Loadout ({0}/{1}) {2}"
                , ModsConfigCollection.Cast<object>().Count()
                , Items.Count()
                , alerts > 0 ? $"!({alerts})" : ""
             );
        }
    }
    public void AlertPropertyChanged()
    {
        RaisePropertyChanged(nameof(ListModsConfigAlerts));
        RaisePropertyChanged(nameof(GetModConfigStaticColor));
        RaisePropertyChanged(nameof(GetModConfigStaticLable));
        RaisePropertyChanged(nameof(GetModListStaticLable));
    }
    #endregion

    public void ModCollectionUpdate(Action action)
    {
        Task.Run(() =>
        {
            App.Current.Dispatcher.Invoke(() => LoadingUI = true);

            action();

            App.Current.Dispatcher.Invoke(() =>
            {
                ModsConfigCollection.Refresh();
                ModsCollection.Refresh();
                AlertPropertyChanged();
            });
            //ModsCollection.Cast<ModModel>().ModListAlertClean();          
            App.Current.Dispatcher.Invoke(() =>
            {
                ModsConfigCollection.Cast<ModModel>().ModListValidation(_settingsService.Settings.GameVersion);
                Items.ModListDuplicateValidation();
                AlertPropertyChanged();
                LoadingUI = false;
            });
        });
    }
    public void LoadMods(string? path = null) => ModCollectionUpdate(() =>
    {
        App.Current.Dispatcher.Invoke(() => Items.ClearSyncProperties(Data_PropertyChanged, c => c?.Data));

        var metaData = _modsServices.LoadModMetaData(path).ToList();
        App.Current.Dispatcher.Invoke(() =>
        {
            foreach (var item in metaData)
                Items.InsertInOrder(item, c => c.Position);
            AlertPropertyChanged();
            LoadData(metaData);
        });

        foreach (var item in _modsServices.LoadModLazy())
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Items.AddOrUpdate(item);
                AlertPropertyChanged();
            });
        }
    });
    public void ReloadModsConfig(string? path = null) => ModCollectionUpdate(() =>
    {
        var config = _modsServices.LoadModsConfig(path);
        App.Current.Dispatcher.Invoke(() =>
        {
            Items.AddOrUpdate(config);
            Items.SortBy(c => c.Position);
        });
    });
    public void ReloadModsData(string? path = null)
    {
        foreach (var item in Items)
        {
            item.Data = null;
        }
        LoadModsData(path);
    }
    public void LoadModsData(string? path = null) => ModCollectionUpdate(() =>
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            var metaData = _modsServices.LoadModData2(path);
            Items.AddOrUpdate(metaData);
            LoadData(Items);
        });
    });

    public void LoadData(IEnumerable<ModModel> modlist)
    {
        var data = modlist.Select(s => s.Data).Where(s => s.IsNotNull());
        foreach (var item in data.SelectMany(s => s.Groups).Where(w => !string.IsNullOrEmpty(w)).Distinct())
            if (!ModGroups.Contains(item))
                ModGroups.Add(item);
        foreach (var item in data.Select(s => s.Color).Where(w => !string.IsNullOrEmpty(w)).Distinct())
            if (!ModColors.Contains(item))
                ModColors.Add(item);
    }

    #region CustomCommand
    public CustomCommand ChangeTheme => new CustomCommand(_themeService.SwitchTheme);
    public CustomCommand OpenSteamLinkCommand => new CustomCommand(FileHelper.OpenSteamLink);
    public CustomCommand OpenLinkCommand => new CustomCommand(FileHelper.OpenLink);
    public CustomCommand ModDetailCommand => new CustomCommand(p => UILock(() =>
    {
        //    new ModDetailWindow(this).ShowDialog(); 
        OpenDialog.ShowDialog<ModDetailWindow>(this);
    }));
    public CustomCommand OptionsCommand => new CustomCommand(p => UILock(() =>
    {
        //    Program.Services.GetRequiredService<SettingsWindow>().ShowDialog();
        //OpenDialog.ShowDialog<SettingsWindow, SettingsViewModel>();
        OpenDialog.ShowServiceDialog<SettingsWindow>();
        //Program.Services.GetRequiredService<SettingsWindow>().ShowDialog();
        RaisePropertyChanged(nameof(ModColumnData));
    }));
    public CustomCommand AboutCommand => new CustomCommand(p => UILock(() =>
    {
    //    Program.Services.GetRequiredService<AboutWindow>().ShowDialog();
        OpenDialog.ShowDialog<AboutWindow>();
    }));
    public CustomCommand ChangeColorCommand => new CustomCommand(p => UILock(() =>
    {
        if (p != null && p is ModModel model) SelectedMod = model;
        if (SelectedMod == null) return;
        ModColors.Clear();
        foreach (var item in Items
            .Where(x => x.Data != null).Where(x => x.Data.IsNotNull())
            .Select(s => s.Data.Color).Where(w => !string.IsNullOrEmpty(w)).Distinct())
        {
            ModColors.Add(item);
        }
        //new ChangeColorWindow(this).ShowDialog();
        OpenDialog.ShowDialog<ChangeColorWindow>(this);       
    }));
    public CustomCommand RefreshCommand => new CustomCommand(p => LoadMods());
    public CustomCommand LoadActiveModlistCommand => new CustomCommand(p => UILock(() => ReloadModsConfig()));
    public CustomCommand SaveActiveModlistCommand => new CustomCommand(p => UILock(() =>
    {
        if (Items.Any())
        {
            _modsServices.SaveConfig(ModsConfigCollection.Cast<ModModel>());
            _modsServices.SaveLocalData(Items);
        }
    }));

    public CustomCommand LoadFileActiveModlistCommand => new CustomCommand(p => UILock(() =>
    {
        var dialog = new OpenFileDialog
        {
            InitialDirectory = _settingsService.Settings.PathModSettingsArchive,
            Title = "Open file",
            Filter = "XML (*.xml)|*.xml|All files (*.*)|*.*",
            FileName = "ModsConfig.xml"
        };
        dialog.ShowDialog((ok,filename) =>
        {
            if (!ok) return;
            if (Path.GetDirectoryName(filename) is string dir && dir != _settingsService.Settings.PathModSettingsArchive) _settingsService.Settings.PathModSettingsArchive = dir;
            if (Items.Any())
                ReloadModsConfig(filename);
            else
                LoadMods(filename);
        });
    }));
    public CustomCommand SaveFileActiveModlistCommand => new CustomCommand(p => UILock(() =>
    {
        var dialog = new SaveFileDialog
        {
            InitialDirectory = _settingsService.Settings.PathModSettingsArchive,
            Title = "Save file",
            Filter = "XML (*.xml)|*.xml|All files (*.*)|*.*",
            DefaultExt = ".xml",
            FileName = "ModsConfig.xml"
        };
        dialog.ShowDialog((ok, filename) =>
        {
            if (!ok) return;
            if (Path.GetDirectoryName(filename) is string dir && dir != _settingsService.Settings.PathModSettingsArchive) _settingsService.Settings.PathModSettingsArchive = dir;
            _modsServices.SaveConfig(ModsConfigCollection.Cast<ModModel>(), filename);
        });
    }));

    public CustomCommand LoadRimpyColorsCommand => new CustomCommand(p => UILock(() =>
    {
        var dialog = new OpenFileDialog
        {
            InitialDirectory = _settingsService.Settings.PathRimpyManager,
            Title = "Open file",
            Filter = "ini |*.ini",
            FileName = "config.ini"
        };
        dialog.ShowDialog((ok, filename) =>
        {
            if (!ok) return;
            if (Path.GetDirectoryName(filename) is string dir && dir != _settingsService.Settings.PathRimpyManager) _settingsService.Settings.PathRimpyManager = dir;
            var data = _modsServices.LoadRimPyColors(filename);
            foreach (var item in Items)
                if (item.Path != null && item.Data != null && data.ContainsKey(item.Path))
                    item.Data.Color = data[item.Path];
        });
    }));
    public CustomCommand ModsToCSVCommand => new CustomCommand(p => UILock(() =>
    {
        if (!Items.Any()) return;
        var dialog = new SaveFileDialog
        {
            Title = "Zapisz plik",
            Filter = "CSV (*.csv)|*.csv",
            DefaultExt = ".xml",
            FileName = "Mods.csv"
        };
        dialog.ShowDialog((ok, filename) =>
        {
            if (!ok) return;
            _modsServices.ExportCSVMods(Items, filename);
        });
    }));

    public CustomCommand LoadMetaDataCommand => new CustomCommand(p => UILock(() =>
    {
        var dialog = new OpenFileDialog
        {
            Title = "Load local data",
            Filter = "JSON (*.json)|*.json|All files (*.*)|*.*",
            FileName = "RimKeeperModOrganizer LocalData.json"
        };
        dialog.ShowDialog((ok, filename) =>
        {
            if (!ok) return;
            if (p is string mode && mode == "reload")
                ReloadModsData(filename);
            else
                LoadModsData(filename);
            _modsServices.SaveLocalData(Items);
        });
    }));
    public CustomCommand SaveMetaDataCommand => new CustomCommand(p => UILock(() =>
    {
        var dialog = new SaveFileDialog
        {
            Title = "Save local data",
            Filter = "JSON (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = ".json",
            FileName = "RimKeeperModOrganizer LocalData.json"
        };
        dialog.ShowDialog((ok, filename) =>
        {
            if (!ok) return;
            _modsServices.SaveLocalData(Items, filename);
        });
    }));
    public CustomCommand RemoveMetaDataCommand => new CustomCommand(p => UILock(() =>
    {
        if (p is ModModel model)
        {
            MessageBox.Show(
                "Remove local mods data?",
                "Mods data action",
                MessageBoxButton.YesNo,
                (result) =>
                {
                    if (result == MessageBoxResult.Yes)
                    {
                        model.Data = null;
                    }
                }
            );
        }
    }));

    public CustomCommand RefreshModCommand => new CustomCommand(p =>
    {
        if (p != null && p is ModModel model)
        {
            Task.Run(() => UILock(() =>
            {
                _modsServices.RefreshMod(model);
            }));
        }
    });

    public CustomCommand SubscribeCommand => new CustomCommand(p =>
    {
        if (p != null && p is ModModel model)
        {
            Task.Run(() => UILock(async () =>
            {
                bool result = _steamService.TryInitializeParse(SelectedMod = model, (c, p) => c.SubscribeItem(p));
                if (result)
                {
                    //string xpath = _settingsService.Settings.PathDirModsSteam + ID
                    string xpath = model.Path;
                    await Task.Delay(1000);
                    bool fileExists = await TaskHelper.WaitDirectoryExist(xpath);
                    var newmod = new ModModel(xpath, ModLocation.Steam);
                    model.Update(newmod);
                    model.RaisePropertyChanged();
                }
            }));
        }
    });
    public CustomCommand UnsubscribeCommand => new CustomCommand(p =>
    {
        if (p != null && p is ModModel model)
        {
            Task.Run(() => UILock(async () =>
            {
                bool result = _steamService.TryInitializeParse(SelectedMod = model, (c, p) => c.UnsubscribeItem(p));
                if (result)
                {
                    //await Task.Delay(1000);
                    //Directory.Delete(model.Path);
                    //bool fileExists = await TaskHelper.WaitDirectoryNotExist(model.Path);
                    model.Location = model.Data != null ? ModLocation.MetaData : ModLocation.Unknow;
                    model.RaisePropertyChanged();
                }
            }));
        }
    });

    public CustomCommand TestCommand => new CustomCommand(p => UILock(async () =>
    {
        var test_list = ModsCollectionColumns;
        var t = Items
            .Select(s => s.About?.SteamId)
            .Where(w => !string.IsNullOrEmpty(w))
            .Take(5)
            .ToList();

        var pid = this.SelectedMod.About.SteamId;
        if (ulong.TryParse(pid, out ulong pid_long))
        {
            //_steamService.TryInitialize(c => c.UnsubscribeItem(pid_long));

            //var a = _steamService.Initialize();
            //var b = _steamService.UnsubscribeItem(pid_long);
            //_steamService.DeInitialize();

            //Task.Run(() =>
            //{
            //    var a = _steamService.Initialize();
            //    var b = _steamService.UnsubscribeItem(pid_long);
            //    _steamService.DeInitialize();
            //});
        }

    }));
    #endregion CustomCommand
}
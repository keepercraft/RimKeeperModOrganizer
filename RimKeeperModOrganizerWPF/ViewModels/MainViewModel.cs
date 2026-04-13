using GongSolutions.Wpf.DragDrop;
using KeeperBaseLib.Model;
using KeeperDataGrid.Extensions;
using KeeperDataGrid.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using RimKeeperModOrganizerLib.Extensions;
using RimKeeperModOrganizerLib.Helpers;
using RimKeeperModOrganizerLib.Models;
using RimKeeperModOrganizerLib.Services;
using RimKeeperModOrganizerWPF.Extensions;
using RimKeeperModOrganizerWPF.Views;
using RimKeeperModOrganizerWPF.Views.Extensions;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
namespace RimKeeperModOrganizerWPF.ViewModels;

public class MainViewModel : PropertyModel, IDropTarget
{
    public ObservableCollection<string> ModGroups { get; set; } = new();
    public ObservableCollection<string> ModColors { get; set; } = new();
    public ObservableCollection<ColumnConfig> ModsCollectionColumns { get; set; } = new();
    public ObservableCollection<ModModel> Items { get; set; } = new();
    public ICollectionView ModsConfigCollection { get; }
    public ICollectionView ModsCollection { get; }
    private ModModel? _selectedMod;
    public ModModel? SelectedMod
    {
        get => _selectedMod;
        set
        {
            _selectedMod = value;
            OnPropertyChanged();
        }
    }
    public bool IsSelectedMod => SelectedMod != null;

    public List<ColumnSettings> ModColumnData => _settingsService.Settings.ModColumnData;
    public MainWidowSettings MainWidowSettings => _settingsService.Settings.MainWidow;

    private readonly JsonAutoSaver _autoSaver;
    private readonly ModsServices _modsServices;
    private readonly SettingsService _settingsService;
    public MainViewModel(ModsServices modsServices, SettingsService SettingsService)
    {
        _modsServices = modsServices;
        _settingsService = SettingsService;
        _autoSaver = new JsonAutoSaver(
            () => DataChanged,
            js => _modsServices.SaveLocalData(Items),
            JsonHelper.Options,
            true);
        _autoSaver.Calculate(false);

        ModColumnData.BindToSettings(ModsCollectionColumns);
        ModsCollection = new ListCollectionView(Items); //CollectionViewSource.GetDefaultView(Items);
        ModsCollection.CombineFilters(LeftViewFilter);
        ModsConfigCollection = new ListCollectionView(Items); //CollectionViewSource.GetDefaultView(Items);
        ModsConfigCollection.CombineFilters(RightViewFilter);
        Items.CollectionChanged += Items_CollectionChanged;
        //ModsConfigCollection.SortDescriptions.Add(new SortDescription(nameof(ModModel.Position), ListSortDirection.Ascending));

        ModTypeIconsList = new List<string>()
        {
            string.Empty,
            ModLocation.DLC.ToString(),
            ModLocation.Local.ToString(),
            ModLocation.Steam.ToString(),
            ModLocation.MetaData.ToString(),
        };
    }

    public IEnumerable<string> ModTypeIconsList { get; set; }

    private bool LeftViewFilter(object obj) => ((ModModel)obj)?.Position == null;
    private bool RightViewFilter(object obj) => ((ModModel)obj)?.Position >= 0;

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
    public List<AlertModel> ListModsConfigAlerts => Items.Cast<ModModel>().SelectMany(x => x.Alerts.Items).ToList();
    public Brush? GetModConfigStaticColor => IsModsConfigAlert ? Brushes.LightCoral : Brushes.Transparent;
    public string GetModConfigStaticLable
    {
        get
        {
            var alerts = Items.Sum(c => c.Alerts.Items.Count);
            return string.Format("Mods ({0}/{1}) {2}"
                , ModsConfigCollection.Cast<object>().Count()
                , Items.Count()
                , alerts > 0 ? $"!({alerts})" : "");
        }
    }
    public void AlertPropertyChanged()
    {
        RaisePropertyChanged(nameof(ListModsConfigAlerts));
        RaisePropertyChanged(nameof(GetModConfigStaticColor));
        RaisePropertyChanged(nameof(GetModConfigStaticLable));
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
    public bool DataChanged = false;
    private void Data_PropertyChanged(object? sender, PropertyChangedEventArgs e) 
        => DataChanged = true;
    private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => e.SyncProperties<ModModel>(Data_PropertyChanged, c => c?.Data);


    #region CustomCommand
    public CustomCommand OpenSteamLinkCommand => new CustomCommand(FileHelper.OpenSteamLink);
    public CustomCommand OpenLinkCommand => new CustomCommand(FileHelper.OpenLink);
    public CustomCommand ModDetailCommand => new CustomCommand(p => UILock(() => new ModDetailWindow(this).ShowDialog()));
    public CustomCommand OptionsCommand => new CustomCommand(p => UILock(() =>
    {
        App.Services.GetRequiredService<SettingsWindow>().ShowDialog();
        RaisePropertyChanged(nameof(ModColumnData));
    }));
    public CustomCommand AboutCommand => new CustomCommand(p => UILock(() =>
    {
        App.Services.GetRequiredService<AboutWindow>().ShowDialog();
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
        new ChangeColorWindow(this).ShowDialog();
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
            DefaultExt = ".xml",
            Multiselect = false,
            FileName = "ModsConfig.xml"
        };
        if (dialog.ShowDialog() ?? false)
        {
            if (Path.GetDirectoryName(dialog.FileName) is string dir && dir != _settingsService.Settings.PathModSettingsArchive) _settingsService.Settings.PathModSettingsArchive = dir;
            if(Items.Any())
                ReloadModsConfig(dialog.FileName);
            else
                LoadMods(dialog.FileName);
        }
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
        if (dialog.ShowDialog() ?? false)
        {
            if (Path.GetDirectoryName(dialog.FileName) is string dir && dir != _settingsService.Settings.PathModSettingsArchive) _settingsService.Settings.PathModSettingsArchive = dir;
            _modsServices.SaveConfig(ModsConfigCollection.Cast<ModModel>(), dialog.FileName);
        }
    }));

    public CustomCommand LoadRimpyColorsCommand => new CustomCommand(p => UILock(() =>
    {
        var dialog = new OpenFileDialog
        {
            InitialDirectory = _settingsService.Settings.PathRimpyManager,
            Title = "Open file",
            Filter = "ini |*.ini",
            DefaultExt = ".ini",
            FileName = "config.ini"
        };
        if (dialog.ShowDialog() ?? false)
        {
            if (Path.GetDirectoryName(dialog.FileName) is string dir && dir != _settingsService.Settings.PathRimpyManager) _settingsService.Settings.PathRimpyManager = dir;
            var data = _modsServices.LoadRimPyColors(dialog.FileName);
            foreach(var item in Items)
                if(item.Path != null && item.Data !=null && data.ContainsKey(item.Path))
                    item.Data.Color = data[item.Path];
        }
    }));
    public CustomCommand ModsToCSVCommand => new CustomCommand(p => UILock(() =>
    {
        IEnumerable<ModModel>? models = p switch
        {
            IEnumerable<ModModel> m => m,
            ICollectionView view => view.Cast<ModModel>(),
            _ => null
        };
        if (models == null || !models.Any()) return;

        var dialog = new SaveFileDialog
        {
            Title = "Zapisz plik",
            Filter = "CSV (*.csv)|*.csv",
            DefaultExt = ".xml",
            FileName = "Mods.csv"
        };
        if (dialog.ShowDialog() ?? false) _modsServices.ExportCSVMods(Items, dialog.FileName);
    }));

    public CustomCommand LoadMetaDataCommand => new CustomCommand(p => UILock(() =>
    {
        var dialog = new OpenFileDialog
        {
            Title = "Load local data",
            Filter = "JSON (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = ".json",
            FileName = "RimKeeperModOrganizer LocalData.json"
        };
        if (dialog.ShowDialog() ?? false)
        {
            if (p is string mode && mode == "reload")
                ReloadModsData(dialog.FileName);
            else
                LoadModsData(dialog.FileName);
            _modsServices.SaveLocalData(Items);
        }
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
        if (dialog.ShowDialog() ?? false)
        {
            _modsServices.SaveLocalData(Items, dialog.FileName);
        }
    }));
    public CustomCommand RemoveMetaDataCommand => new CustomCommand(p => UILock(() =>
    {
        if (p is ModModel model)
        {
            var result = MessageBox.Show(
                "Remove local mods data?",
                "Mods data action",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );
            if (result == MessageBoxResult.Yes)
                model.Data = null;
        }
    }));

    public CustomCommand TestCommand => new CustomCommand(p => UILock(async () =>
    {
        var test_list = ModsCollectionColumns;

        var t = Items
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

    #region Drag&Dro
    public void DragOver(IDropInfo dropInfo)
    {
        if (dropInfo.Data is ModModel || dropInfo.Data is IEnumerable<object>)
        {
            dropInfo.Effects = DragDropEffects.Move;
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
        }
    }
    public void Drop(IDropInfo dropInfo)
    {
        var itemsToMove = DefaultDropHandler.ExtractData(dropInfo.Data).Cast<ModModel>().Reverse();
        if (!itemsToMove.Any()) return;
        bool isDroppingToAssigned = dropInfo.TargetCollection == ModsConfigCollection;
        int insertIndex = dropInfo.InsertIndex;
        int targetIndex = dropInfo.UnfilteredInsertIndex;

        foreach (var item in itemsToMove)
        {
            if (!isDroppingToAssigned) item.Position = null; else item.Position = 0;
            int currentIndex = Items.IndexOf(item);
            if (currentIndex == -1) continue;
            int actualTarget = currentIndex < targetIndex ? targetIndex - 1 : targetIndex;
            actualTarget = Math.Max(0, Math.Min(actualTarget, Items.Count - 1));
            Items.Move(currentIndex, actualTarget);
        }

        if (dropInfo.VisualTarget is DataGrid targetGrid)
        {
            targetGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                targetGrid.SelectedItems.Clear();
                foreach (var item in itemsToMove)
                {
                    targetGrid.SelectedItems.Add(item);
                }
                if (itemsToMove.Any())
                {
                    targetGrid.ScrollIntoView(itemsToMove.First());
                    targetGrid.Focus();
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        int i = 0;
        var ttt = ModsConfigCollection.Cast<ModModel>().Where(w => w.Position != null);
        foreach (var item in ttt)
        {
            item.Position = Items.IndexOf(item);
        }

        itemsToMove.ModListAlertClean();

            //Items.Cast<ModModel>().ModListValidation();
        ModsConfigCollection.Cast<ModModel>().ModListValidation(_settingsService.Settings.GameVersion);
        AlertPropertyChanged();
      //  ModsCollection.Refresh();
      //  ModsConfigCollection.Refresh();
    }
    #endregion
}
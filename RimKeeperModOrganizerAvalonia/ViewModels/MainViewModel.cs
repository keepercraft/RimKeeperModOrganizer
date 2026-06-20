using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using KeeperBaseSharedLib.Models;
using KeeperBaseSheredLib;
using KeeperDataGridAvalonia.Extensions;
using KeeperDataGridAvalonia.Models;
using Microsoft.Extensions.DependencyInjection;
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
using System.Diagnostics;
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

    public ObservableCollection<ModModel> SelectedMods { get; set; } = new();  //public IEnumerable<ModModel> SelectedMods => SelectedItems?.Cast<ModModel>() ?? (SelectedMod != null ? [SelectedMod] : Enumerable.Empty<ModModel>());
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
    public bool IsSelectedMod => SelectedMod != null;//|| SelectedMods.Any();
    public ModDataCopyModel? ModDataCopy { get; set; }

    public List<ColumnSettings> ModColumnData => _settingsService.Settings.ModColumnData;
    public MainWidowSettings MainWidowSettings => _settingsService.Settings.MainWidow;
    public List<string> ModTypeIconsList { get; set; } = new();
    public List<string> ModColorIconsList => [string.Empty, "#", ..ModColors];
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

    public ObservableCollection<string> DummyGroups { get; set; } = new();
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
        ModColors.CollectionChanged += (s, e) =>
        {
            //RaisePropertyChanged(nameof(ModColorIconsList));
        };
        ModTypeIconsList.AddRange(new[]
        {
            string.Empty,
            ModLocation.DLC.ToString(),
            ModLocation.Local.ToString(),
            ModLocation.Steam.ToString(),
            ModLocation.MetaData.ToString(),
        });
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
        catch (Exception ex)
        {
            Debug.WriteLine($"UILock ERROR {ex.Message}");
        }
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
        //RaisePropertyChanged(nameof(ModColorIconsList));
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
                //AlertPropertyChanged();
            });
            //ModsCollection.Cast<ModModel>().ModListAlertClean();          
            App.Current.Dispatcher.Invoke(() =>
            {
                ModsConfigCollection.Cast<ModModel>().ModListValidation(_settingsService.Settings.GameVersion);
                Items.ModListDuplicateValidation();
                AlertPropertyChanged();
                LoadingUI = false;
            });
            Debug.WriteLine($"ModCollectionUpdate TASK FINISH");
        });
    }
    public void LoadMods(string? path = null) => ModCollectionUpdate(async () =>
    {
        App.Current.Dispatcher.Invoke(() => Items.ClearSyncProperties(Data_PropertyChanged, c => c?.Data));

        //var metaData = _modsServices.LoadModMetaData(path).ToList();
        //App.Current.Dispatcher.Invoke(() =>
        //{
        //    foreach (var item in metaData)
        //        Items.InsertInOrder(item, c => c.Position);
        //    AlertPropertyChanged();
        //    LoadData(metaData);
        //});

        var metaData = _modsServices.LoadModData2(path);
        App.Current.Dispatcher.Invoke(() =>
        {      
            Items.AddOrUpdate(metaData);
            LoadData(Items);
        });

        foreach (var item in _modsServices.LoadModLazy())
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Items.AddOrUpdate(item);
                AlertPropertyChanged();
            });
        }

        var config = _modsServices.LoadModsConfig(path);
        App.Current.Dispatcher.Invoke(() =>
        {
            Items.AddOrUpdate(config);
            Items.SortBy(c => c.Position);
        });
    });
    public void ReloadModsConfig(string? path = null) => ModCollectionUpdate(() =>
    {

        var config = _modsServices.LoadModsConfig(path);
        App.Current.Dispatcher.Invoke(() =>
        {
            Items.ResetPosition();
            Items.AddOrUpdate(config);
            Items.SortBy(c => c.Position);
        });
        Debug.WriteLine($"ReloadModsConfig for {config.ActiveMods.Count}");
    });
    public void ReloadModsData(string? path = null)
    {
        foreach (var item in Items)
        {
            item.Data = null;
        }
        LoadModsData(path);
    }
    public void LoadModsData(string? path = null) => ModCollectionUpdate(async () =>
    {
        var metaData = _modsServices.LoadModData2(path);
        App.Current.Dispatcher.Invoke(() =>
        {
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
    public CustomCommand RunAppCommand => new CustomCommand(()=>FileHelper.OpenRimworldGame(_settingsService.Settings.PathDirGame));
    public CustomCommand MoveToLocalCommand => new CustomCommand(p => UILock(() =>
    {
        foreach (var mod in SelectedMods)
        {




        }
    }));
    public CustomCommand ModDetailCommand => new CustomCommand(p => UILock(() =>
    {
        //    new ModDetailWindow(this).ShowDialog(); 
        DialogService.ShowDialogWindow<ModDetailWindow>(this);
    }));
    public CustomCommand OptionsCommand => new CustomCommand(p =>//UILock(() =>
    {
        //    Program.Services.GetRequiredService<SettingsWindow>().ShowDialog();
        //OpenDialog.ShowDialog<SettingsWindow, SettingsViewModel>();
        DialogService.ShowDialogWindow<SettingsWindow>();
        //Program.Services.GetRequiredService<SettingsWindow>().ShowDialog();
        RaisePropertyChanged(nameof(ModColumnData));
    }
    //)
        );
    public CustomCommand AboutCommand => new CustomCommand(p => UILock(() =>
    {
        //    Program.Services.GetRequiredService<AboutWindow>().ShowDialog();
        DialogService.ShowDialogWindow<AboutWindow>();
    }));
    public CustomCommand ChangeColorCommand => new CustomCommand(p => UILock(() =>
    {
        //if (p != null && p is ModModel model) SelectedMod = model;
        if (!SelectedMods.Any() || SelectedMod == null) return;
        DialogService.ShowDialogWindow<ChangeColorWindow>(this);       
    }));
    public CustomCommand RefreshCommand => new CustomCommand(p => LoadMods());
    public CustomCommand LoadActiveModlistCommand => new CustomCommand(p => UILock(() => ReloadModsConfig()));
    public CustomCommand SaveActiveModlistCommand => new CustomCommand(p => UILock(() =>
    {
        if (Items.Any())
        {
            _modsServices.SaveConfig(Items);
            _modsServices.SaveLocalData(Items);
        }
    }));

    public CustomCommand LoadFileActiveModlistCommand => new CustomCommand(p => UILock(() =>
    {
        //var dialog = new OpenFileDialog
        //{
        //    InitialDirectory = _settingsService.Settings.PathModSettingsArchive,
        //    Title = "Open file",
        //    Filter = "XML (*.xml)|*.xml|All files (*.*)|*.*",
        //    FileName = "ModsConfig.xml"
        //};
        //dialog.ShowDialog((ok,filename) =>
        //{
        //    if (!ok) return;
        //    if (Path.GetDirectoryName(filename) is string dir && dir != _settingsService.Settings.PathModSettingsArchive) _settingsService.Settings.PathModSettingsArchive = dir;
        //    if (Items.Any())
        //        ReloadModsConfig(filename);
        //    else
        //        LoadMods(filename);
        //});
        StorageDialog.OpenFileDialog(
            filters: "XML (*.xml)|*.xml|All files (*.*)|*.*",
            SuggestedFileName: "ModsConfig.xml",
            startLocation: _settingsService.Settings.PathModSettingsArchive
        ).ContinueWith(t =>
        {
            var result = t.Result;
            if (result == null || !result.Any()) return;
            var filename = result.First();
            if (Path.GetDirectoryName(filename) is string dir && dir != _settingsService.Settings.PathModSettingsArchive) _settingsService.Settings.PathModSettingsArchive = dir;
            if (Items.Any())
                ReloadModsConfig(filename);
            else
                LoadMods(filename);
        });
    }));
    public CustomCommand SaveFileActiveModlistCommand => new CustomCommand(p => UILock(() =>
    {
        //var dialog = new SaveFileDialog
        //{
        //    InitialDirectory = _settingsService.Settings.PathModSettingsArchive,
        //    Title = "Save file",
        //    Filter = "XML (*.xml)|*.xml|All files (*.*)|*.*",
        //    DefaultExt = ".xml",
        //    FileName = "ModsConfig.xml"
        //};
        //dialog.ShowDialog((ok, filename) =>
        //{
        //    if (!ok) return;
        //    if (Path.GetDirectoryName(filename) is string dir && dir != _settingsService.Settings.PathModSettingsArchive) _settingsService.Settings.PathModSettingsArchive = dir;
        //    _modsServices.SaveConfig(Items, filename);
        //});
        StorageDialog.SaveFileDialog(
            filters: "XML (*.xml)|*.xml|All files (*.*)|*.*",
            SuggestedFileName: "ModsConfig.xml",
            defaultExtension: ".xml",
            startLocation: _settingsService.Settings.PathModSettingsArchive
        ).ContinueWith(t =>
        {
            var filename = t.Result;
            if (string.IsNullOrEmpty(filename)) return;
            if (Path.GetDirectoryName(filename) is string dir && dir != _settingsService.Settings.PathModSettingsArchive) _settingsService.Settings.PathModSettingsArchive = dir;
            _modsServices.SaveConfig(Items, filename);
        });
    }));

    public CustomCommand LoadRimpyColorsCommand => new CustomCommand(p => UILock(() =>
    {
        //var dialog = new OpenFileDialog
        //{
        //    InitialDirectory = _settingsService.Settings.PathRimpyManager,
        //    Title = "Open file",
        //    Filter = "ini |*.ini",
        //    FileName = "config.ini"
        //};
        //dialog.ShowDialog((ok, filename) =>
        //{
        //    if (!ok) return;
        //    if (Path.GetDirectoryName(filename) is string dir && dir != _settingsService.Settings.PathRimpyManager) _settingsService.Settings.PathRimpyManager = dir;
        //    var data = _modsServices.LoadRimPyColors(filename);
        //    foreach (var item in Items)
        //        if (item.Path != null && item.Data != null && data.ContainsKey(item.Path))
        //            item.Data.Color = data[item.Path];
        //});
        StorageDialog.OpenFileDialog(
            filters: "ini |*.ini",
            SuggestedFileName: "config.ini",
            startLocation: _settingsService.Settings.PathRimpyManager
            ).ContinueWith(t =>
        {
            var result = t.Result;
            if (result == null || !result.Any()) return;
            var filename = result.First();
            if (Path.GetDirectoryName(filename) is string dir && dir != _settingsService.Settings.PathRimpyManager) _settingsService.Settings.PathRimpyManager = dir;
            var data = _modsServices.LoadRimPyColors(filename);
            foreach (var item in Items)
                if (item.Path != null && item.Data != null && data.ContainsKey(item.Path))
                    item.Data.Color = data[item.Path];
        });
    }));
    public CustomCommand ModsToCSVCommand => new CustomCommand(p => UILock(() =>
    {
        //if (!Items.Any()) return;
        //var dialog = new SaveFileDialog
        //{
        //    Title = "Zapisz plik",
        //    Filter = "CSV (*.csv)|*.csv",
        //    DefaultExt = ".xml",
        //    FileName = "Mods.csv"
        //};
        //dialog.ShowDialog((ok, filename) =>
        //{
        //    if (!ok) return;
        //    _modsServices.ExportCSVMods(Items, filename);
        //});
        StorageDialog.SaveFileDialog(
            filters: "CSV (*.csv)|*.csv",
            SuggestedFileName: "Mods.csv",
            defaultExtension: ".csv"
        ).ContinueWith(t =>
        {
            var filename = t.Result;
            if (string.IsNullOrEmpty(filename)) return;
            _modsServices.ExportCSVMods(Items, filename);
        });
    }));

    public CustomCommand LoadMetaDataCommand => new CustomCommand(p => UILock(() =>
    {
        //var dialog = new OpenFileDialog
        //{
        //    Title = "Load local data",
        //    Filter = "JSON (*.json)|*.json|All files (*.*)|*.*",
        //    FileName = "RimKeeperModOrganizer LocalData.json"
        //};
        //dialog.ShowDialog((ok, filename) =>
        //{
        //    if (!ok) return;
        //    if (p is string mode && mode == "reload")
        //        ReloadModsData(filename);
        //    else
        //        LoadModsData(filename);
        //    _modsServices.SaveLocalData(Items);
        //});
        StorageDialog.OpenFileDialog(
            filters: "JSON (*.json)|*.json|All files (*.*)|*.*",
            SuggestedFileName: "RimKeeperModOrganizer LocalData.json"
            ).ContinueWith(t =>
        {
            var result = t.Result;
            if (result == null || !result.Any()) return;
            var filename = result.First();
            if (p is string mode && mode == "reload")
                ReloadModsData(filename);
            else
                LoadModsData(filename);
            _modsServices.SaveLocalData(Items);
        });
    }));
    public CustomCommand SaveMetaDataCommand => new CustomCommand(p => UILock(() =>
    {
        //var dialog = new SaveFileDialog
        //{
        //    Title = "Save local data",
        //    Filter = "JSON (*.json)|*.json|All files (*.*)|*.*",
        //    DefaultExt = ".json",
        //    FileName = "RimKeeperModOrganizer LocalData.json"
        //};
        //dialog.ShowDialog((ok, filename) =>
        //{
        //    if (!ok) return;
        //    _modsServices.SaveLocalData(Items, filename);
        //});
        StorageDialog.SaveFileDialog(
            filters: "JSON (*.json)|*.json|All files (*.*)|*.*",
            SuggestedFileName: "RimKeeperModOrganizer LocalData.json",
            defaultExtension: ".json"
        ).ContinueWith(t =>
        {
            var filename = t.Result;
            if (string.IsNullOrEmpty(filename)) return;
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
                        foreach (var model in SelectedMods)
                        {
                            model.Data = null;
                        }
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
                foreach (var model in SelectedMods)
                {
                    _modsServices.RefreshMod(model);
                }
                //_modsServices.RefreshMod(model);
            }));
        }
    });

    public CustomCommand SubscribeCommand => new CustomCommand(p =>
    {
        if (p != null && p is ModModel model)
        {
            Task.Run(() => UILock(async () =>
            {
                foreach (var model in SelectedMods)
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
                foreach (var model in SelectedMods)
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
                }
            }));
        }
    });

    public CustomCommand CopyModDataCommand => new CustomCommand(p =>
    {
        if (p != null && p is ModModel model)
        {
            ModDataCopy = model.Copy();
        }
    });

    public CustomCommand PasteModDataCommand => new CustomCommand(p =>
    {
        if (p != null && p is ModModel)
        {
            foreach (var model in SelectedMods)
            {
                model.Paste(ModDataCopy);
            } 
        }
    });

    public CustomCommand TestCommand => new CustomCommand(async p =>// UILock(async () =>
    {
        return;

        var data = await StorageDialog.OpenFolderDialog( 
            startLocation: _settingsService.Settings.PathDirSteam
            );
        return;


        var window = Program.Services.GetRequiredService<MainWindow>();
        var topLevel = TopLevel.GetTopLevel(window);
        var options = new FolderPickerOpenOptions { Title = "test" };
        options.SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(_settingsService.Settings.PathDirSteam);
        await Dispatcher.UIThread.InvokeAsync(() => { });
        var dialog = await topLevel.StorageProvider.OpenFolderPickerAsync(options);
        await Dispatcher.UIThread.InvokeAsync(() => { });
        var paths = dialog.Select(s => s.Path.LocalPath).ToList();
        var counts = dialog.Count;

        await Task.Yield();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        Dispatcher.UIThread.Post(() =>
        {
            //var st = new SettingsViewModel(_settingsService);
            //var tt = new SettingsWindow(st);
            var tt = new AboutWindow();
           
            tt.Closing += (sender, e) =>
            {
                if (sender is Window w && !w.Tag?.Equals("closing") == true)
                {
                    e.Cancel = true; // Zatrzymaj synchroniczne zamknięcie
                    w.Tag = "closing";

                    // Zamknij w następnym ticku, poza WndProc
                    Dispatcher.UIThread.Post(() => w.Close(), DispatcherPriority.Background);
                }
            };
            tt.Closed += (_, _) =>
            {
                var stackTrace = new System.Diagnostics.StackTrace(true);
                Debug.WriteLine($"[Closed] UI thread: {Dispatcher.UIThread.CheckAccess()}, Stack:\n{stackTrace}");
            };

            var watchdog = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(100);
                    var sw = Stopwatch.StartNew();

                    await Dispatcher.UIThread.InvokeAsync(() => sw.Stop());

                    if (sw.ElapsedMilliseconds > 100)
                    {
                        Debug.WriteLine($"[WATCHDOG] UI blocked {sw.ElapsedMilliseconds}ms");
                        // Zrób snapshot wątku
                        foreach (ProcessThread t in Process.GetCurrentProcess().Threads)
                        {
                            Debug.WriteLine($"  Thread {t.Id} state: {t.ThreadState}");
                        }
                    }
                }
            });


            tt.Show(window);
        }, DispatcherPriority.Background);


        // 2. Pokazujemy nasze własne okno dialogowe i czekamy na nie
        //OpenDialog.ShowServiceDialog<SettingsWindow>();

        //var fds = new FolderDialogService();
        //fds.ShowDialog("test", Path.GetFullPath(_settingsService.Settings.PathDirSteam), (ok, path) =>
        //{
        //    if (!ok) return;
        //});

        //new OpenFolderDialog
        //{
        //    Title = "test",
        //    InitialDirectory = Path.GetFullPath(_settingsService.Settings.PathDirSteam)
        //}.ShowDialog((ok, path) =>
        //{
        //    if (!ok) return;
        //});

        //var test_list = ModsCollectionColumns;
        //var t = Items
        //    .Select(s => s.About?.SteamId)
        //    .Where(w => !string.IsNullOrEmpty(w))
        //    .Take(5)
        //    .ToList();

        //var pid = this.SelectedMod.About.SteamId;
        //if (ulong.TryParse(pid, out ulong pid_long))
        //{
        //    //_steamService.TryInitialize(c => c.UnsubscribeItem(pid_long));

        //    //var a = _steamService.Initialize();
        //    //var b = _steamService.UnsubscribeItem(pid_long);
        //    //_steamService.DeInitialize();

        //    //Task.Run(() =>
        //    //{
        //    //    var a = _steamService.Initialize();
        //    //    var b = _steamService.UnsubscribeItem(pid_long);
        //    //    _steamService.DeInitialize();
        //    //});
        //}

    }//)
    );
    #endregion CustomCommand
}
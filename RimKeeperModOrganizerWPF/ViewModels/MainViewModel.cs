using KeeperBaseLib.Model;
using KeeperBaseWPFLib.MVVM;
using Microsoft.Win32;
using RimKeeperModOrganizerLib.Helpers;
using RimKeeperModOrganizerLib.Models;
using RimKeeperModOrganizerWPF.Extensions;
using RimKeeperModOrganizerWPF.Services;
using RimKeeperModOrganizerWPF.Views;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Policy;
using System.Windows.Input;
using System.Windows.Media;

namespace RimKeeperModOrganizerWPF.ViewModels;

public class MainViewModel : PropertyModel
{
    private readonly SettingsService _settingsService;
    public MainViewModel() : this(App.SettingsService) { }
    public MainViewModel(SettingsService SettingsService)
    {
        _settingsService = SettingsService;
        ModsConfigCollection.CollectionChanged += ModsConfigCollection_CollectionChanged;
        LoadModsFromLocalAsync();
    }

    public Task LoadModsFromLocalAsync(string? path = null)
    {
        LoadModsFromLocalPath = path;
        return Task.Run(LoadModsFromLocal);
    }
    
    public bool LoadModsFromLocalRunning = false;
    public string? LoadModsFromLocalPath = null;
    public void LoadModsFromLocal()
    {
        if (LoadModsFromLocalRunning) return;
        try
        {
            LoadModsFromLocalRunning = true;
            LoadingUI = true;
            RaisePropertyChanged(nameof(IsUIListEnabled));

            
            var aboutPath = FileHelper.GetModsConfig(_settingsService.Settings.PathDirGameConfig) ?? FileHelper.GetModsConfig();
            if (aboutPath == null) return;
            // ModsConfigModel? mods = XMLHelper.LoadModsConfigFromConfigPath();
            ModsConfigModel? mods = XMLHelper.LoadModsConfig(aboutPath);
            LocalDataListModel? modsData = XMLHelper.LoadLocalData(_settingsService.Settings.PathModData) ?? new LocalDataListModel();
            foreach (var mod in modsData.ModDataList.Where(w => w.Groups.Any()))
            {

            }

            App.Current.Dispatcher.Invoke(() =>
            {
                ModsCollection.Clear();
                ModsConfigCollection.Clear();
            });

           // foreach (ModModel mod in FileHelper.FindRimWorldAllMods())
            foreach (ModModel mod in _settingsService.FindRimWorldAllMods())
            {
                mod.Position = mods?.Position(mod.About?.PackageId) ?? -1;
                mod.TrySet(modsData);
                if (mod.Data != null)
                    mod.Data.PropertyChanged += ModData_PropertyChanged;
                if (mod.Position >= 0) mod.Selected = true;

                App.Current.Dispatcher.Invoke(() =>
                {
                    if (mod.Selected)
                        ModsConfigCollection.Add(mod);
                    else
                        ModsCollection.Add(mod);
                });
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                ModsConfigCollection.SortCollectionByConfig(mods);
                ModListValidation();

                foreach (var item in modsData.ModDataList.SelectMany(s => s.Groups).Where(w => !string.IsNullOrEmpty(w)).Distinct())
                    if (!ModGroups.Contains(item))
                        ModGroups.Add(item);
                foreach (var item in modsData.ModDataList.Select(s => s.Color).Where(w => !string.IsNullOrEmpty(w)).Distinct())
                    if (!ModColors.Contains(item))
                        ModColors.Add(item);
            });

            mods = null;
            modsData = null;
            GC.Collect();
        }
        catch (Exception ex) 
        {
        }
        finally
        {
            LoadModsFromLocalPath = null;
            LoadModsFromLocalRunning = false;
            LoadingUI = false;
            RaisePropertyChanged(nameof(IsUIListEnabled));
            OnPropertyChanged(nameof(GetModConfigStaticLable));
        }
    }

    private void ModData_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {

    }

    public bool LoadingUI = false;
    public bool IsUIListEnabled => !LoadingUI;

    public ObservableCollection<string> ModGroups { get; set; } = new();
    public ObservableCollection<string> ModColors { get; set; } = new();

    // public ICollectionView ModsConfigCollectionView { get; }
    //public ICollectionView ModsView { get; }
    public ObservableCollection<ModModel> ModsConfigCollection { get; } = new();
    public ObservableCollection<ModModel> ModsCollection { get; } = new();
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
                RaisePropertyChanged(nameof(GetModConfigStaticLable));
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedMod)));
            }
        }
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
                collection[i].OnPropertyChanged(nameof(ModModel.Position));
            }
            ModListValidation();
        }
    }

    private void ModsCollection_CollectionChanged2(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is ObservableCollection<ModModel> collection)
        {
            var new_item = collection[e.NewStartingIndex];
            var old_item = collection[e.OldStartingIndex];
            if (new_item.Selected && old_item.Selected)
            {
                for (int i = 0, n = 0; i < collection.Count; i++)
                {
                    if (!collection[i].Selected) continue;
                    collection[i].Position = n++;
                    collection[i].OnPropertyChanged(nameof(ModModel.Position));
                }
                //ModsConfigCollectionView.SortDescriptions.Add(new SortDescription(nameof(ModModel.Position), ListSortDirection.Ascending));
            }
        }
        //ModsConfigCollectionView.Refresh(); 
    }

    private void ModsCollection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if(sender is ObservableCollection<ModModel> collection)
        {
            var positionon = collection == ModsConfigCollection;
                for (int i = 0; i < collection.Count; i++)
                {
                    collection[i].Position = positionon ? i : -1;
                    collection[i].OnPropertyChanged(nameof(ModModel.Position));
                }        
        }
        //switch (e.Action)
        //{
        //    case NotifyCollectionChangedAction.Add:
        //        Console.WriteLine($"Dodano: {e.NewItems[0]}");
        //        break;
        //    case NotifyCollectionChangedAction.Remove:
        //        Console.WriteLine($"Usunięto: {e.OldItems[0]}");
        //        break;
        //    case NotifyCollectionChangedAction.Move:
        //        Console.WriteLine($"Przeniesiono z {e.OldStartingIndex} do {e.NewStartingIndex}");
        //        break;
        //    case NotifyCollectionChangedAction.Replace:
        //        Console.WriteLine("Podmieniono element");
        //        break;
        //    case NotifyCollectionChangedAction.Reset:
        //        Console.WriteLine("Reset listy");
        //        break;
        //}
    }

    public CustomCommand OpenLinkCommand => new CustomCommand(p =>
    {
        if(p is string txt)
            Process.Start(new ProcessStartInfo
            {
                FileName = txt,
                UseShellExecute = true
            });
    });

    public CustomCommand OptionsCommand => new CustomCommand(p =>
    {
        var sw = new SettingsWindow()
        {
             DataContext = new SettingsViewModel()
        };
        var t = sw.ShowDialog();     
    });
    public CustomCommand ChangeColorCommand => new CustomCommand(p =>
    {
        if (SelectedMod == null) return;
        var sw = new ChangeColorWindow(SelectedMod);
        var t = sw.ShowDialog();
    });
    public CustomCommand RefreshCommand => new CustomCommand(p => LoadModsFromLocalAsync());
    public CustomCommand LoadConfigCommand => new CustomCommand(p => 
    {
        var dialog = new OpenFileDialog
        {
            Title = "Wybierz plik",
            Filter = "XML (*.xml)|*.xml|Wszystkie pliki (*.*)|*.*",
            Multiselect = false
        };
        if (dialog.ShowDialog() ?? false) LoadModsFromLocalAsync(dialog.FileName);
    });
    public CustomCommand SaveConfigCommand => new CustomCommand(p => 
    {
        var dialog = new SaveFileDialog
        {
            Title = "Zapisz plik",
            Filter = "XML (*.xml)|*.xml",
            DefaultExt = ".xml",
            FileName = "ModsConfig.xml"
        };
        if (dialog.ShowDialog()??false) SaveConfig(dialog.FileName);
    });
    public CustomCommand SaveCommand => new CustomCommand(p => Save());
    private void Save()
    {
        try
        {
            LoadingUI = true;
            SaveConfig();

            LocalDataListModel modsData = new LocalDataListModel();
            foreach (var item in ModsCollection)
            {
                if (string.IsNullOrEmpty(item.Data?.Group)) continue;
                modsData.ModDataList.Add(item.Data);
            }
            foreach (var item in ModsConfigCollection)
            {
                if (string.IsNullOrEmpty(item.Data?.Group)) continue;
                modsData.ModDataList.Add(item.Data);
            }
            SaveLocalData();
        }
        catch (Exception ex) {}
        finally { LoadingUI = false; }
    }
    private void SaveConfig(string? path = null)
    {
        var aboutPath = FileHelper.GetModsConfig();
        if (aboutPath != null)
        {
            ModsConfigModel? mods = XMLHelper.LoadModsConfig(aboutPath);
            mods.Version = _settingsService.Settings.GameVersion;
            mods.ActiveMods = ModsConfigCollection.Where(x => x.Data != null).Select(x => x.Data.PackageId).ToList();
            XMLHelper.SaveModsConfig(path??aboutPath, mods);
        }
    }
    private void SaveLocalData()
    {
        LocalDataListModel? modsData = XMLHelper.LoadLocalData(_settingsService.Settings.PathModData) ?? new LocalDataListModel();
        modsData.ModDataList.Clear();
        foreach (var item in ModsConfigCollection.Where(x => x.Data != null).Where(x => x.Data.NotNull))
        {
            modsData.ModDataList.Add(item.Data);
        }
        foreach (var item in ModsCollection.Where(x => x.Data != null).Where(x => x.Data.NotNull))
        {
            modsData.ModDataList.Add(item.Data);
        }
        XMLHelper.SaveLocalData(modsData, _settingsService.Settings.PathModData);
    }


    public bool IsModsConfigAlert => ModsConfigCollection.Where(x => x.Alert !=null).Any(x => x.Alert.Any());
    public Brush? GetModConfigStaticColor => IsModsConfigAlert ? Brushes.LightCoral : Brushes.Transparent;
    public string GetModConfigStaticLable => string.Format("Mods ({0}/{1})", ModsConfigCollection.Count, ModsCollection.Count);

    public void ModListValidation()
    {
        ModsConfigCollection.SortBy(c => c.Position);

        int index = 0;
        foreach (var mod in ModsConfigCollection)
        {
            mod.Alert.Clear();
            if (string.IsNullOrEmpty(mod.About?.PackageId)) continue;

            if (mod.About?.ModDependencies is { } mod_deps)
                foreach (var mod_dep in mod_deps)
                {
                    if (string.IsNullOrEmpty(mod_dep.PackageId)) continue;
                    if (!ModsConfigCollection.Any(a => a.About?.PackageId == mod_dep.PackageId))
                    {
                        mod.Alert.Add("Dependency:"+ mod_dep.PackageId);
                    }
                }

            if (mod.About?.IncompatibleWith is { } mod_incom_deps)
                foreach (var mod_dep_id in mod_incom_deps)
                {
                    if (string.IsNullOrEmpty(mod_dep_id)) continue;
                    if (ModsConfigCollection.Any(a => a.About?.PackageId == mod_dep_id))
                    {
                        mod.Alert.Add("IncompatibleWith:" + mod_dep_id);
                    }
                }

            if (mod.About?.LoadAfter is { } mod_after_deps)
                foreach (var mod_dep_id in mod_after_deps)
                {
                    if (string.IsNullOrEmpty(mod_dep_id)) continue;
                    if (ModsConfigCollection.Skip(index+1).Where(a => a.About?.PackageId == mod_dep_id).Any(a => a.About?.PackageId == mod_dep_id))
                    {
                        mod.Alert.Add("LoadAfter:" + mod_dep_id);
                    }
                }

            if (mod.About?.LoadBefore is { } mod_before_deps)
                foreach (var mod_dep_id in mod_before_deps)
                {
                    if (string.IsNullOrEmpty(mod_dep_id)) continue;
                    if (ModsConfigCollection.Take(index).Where(a => a.About?.PackageId == mod_dep_id).Any(a => a.About?.PackageId == mod_dep_id))
                    {
                        mod.Alert.Add("LoadBefore:" + mod_dep_id);
                    }
                }

            mod.OnPropertyChanged(nameof(ModModel.HasAlert));
            index++;
        }

        RaisePropertyChanged(nameof(GetModConfigStaticColor));
        RaisePropertyChanged(nameof(GetModConfigStaticLable));
    }
}
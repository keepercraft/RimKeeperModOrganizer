using KeeperBaseLib.Model;
using KeeperBaseWPFLib.MVVM;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using RimKeeperModOrganizerLib.Extensions;
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

    public bool LoadingUI = false;
    public bool IsUIListEnabled => !LoadingUI;
    public bool IsModsConfigAlert => ModsConfigCollection.Where(x => x.Alert != null).Any(x => x.Alert.Any());
    public Brush? GetModConfigStaticColor => IsModsConfigAlert ? Brushes.LightCoral : Brushes.Transparent;
    public string GetModConfigStaticLable => string.Format("Mods ({0}/{1})", ModsConfigCollection.Count, ModsCollection.Count);

    private readonly ModsServices _modsServices;
    private readonly SettingsService _settingsService;
    public MainViewModel(ModsServices modsServices, SettingsService SettingsService)
    {
        _modsServices = modsServices;
        _settingsService = SettingsService;
        ModsConfigCollection.CollectionChanged += ModsConfigCollection_CollectionChanged;
    }

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
                RaisePropertyChanged(nameof(GetModConfigStaticColor));
                RaisePropertyChanged(nameof(GetModConfigStaticLable));
            });
            foreach (var item in _modsServices.LoadMods(path))
            {
                if (item != null)
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        //AllMods.Add(item);
                        if (item.Selected)
                        {
                            ModsConfigCollection.Add(item);
                            RaisePropertyChanged(nameof(GetModConfigStaticColor));
                            RaisePropertyChanged(nameof(GetModConfigStaticLable));
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
                RaisePropertyChanged(nameof(GetModConfigStaticColor));
                RaisePropertyChanged(nameof(GetModConfigStaticLable));
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
                collection[i].OnPropertyChanged(nameof(ModModel.Position));
            }
            ModsConfigCollection.ModListValidation();
            RaisePropertyChanged(nameof(GetModConfigStaticColor));
            RaisePropertyChanged(nameof(GetModConfigStaticLable));
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
    public CustomCommand OptionsCommand => new CustomCommand(p =>
    {
        App.Services.GetRequiredService<SettingsWindow>().ShowDialog();
        RaisePropertyChanged(nameof(ModColumnData));
    });
    public CustomCommand ChangeColorCommand => new CustomCommand(p =>
    {
        if (SelectedMod == null) return;

        ModColors.Clear();
        foreach (var item in ModsConfigCollection.Union(ModsCollection)
            .Where(x => x.Data != null).Where(x => x.Data.NotNull)
            .Select(s => s.Data.Color).Where(w => !string.IsNullOrEmpty(w)).Distinct())
        {
            ModColors.Add(item);
        }
        // new ChangeColorWindow(SelectedMod, ModColors).ShowDialog();
        new ChangeColorWindow(this).ShowDialog();
    });
    public CustomCommand RefreshCommand => new CustomCommand(p => LoadMods());
    public CustomCommand LoadConfigCommand => new CustomCommand(p => 
    {
        try
        {
            LoadingUI = true;
            var dialog = new OpenFileDialog
            {
                Title = "Wybierz plik",
                Filter = "XML (*.xml)|*.xml|Wszystkie pliki (*.*)|*.*",
                Multiselect = false
            };
            if (dialog.ShowDialog() ?? false) LoadMods(dialog.FileName);
        }
        catch (Exception ex) { }
        finally { LoadingUI = false; }
    });
    public CustomCommand SaveConfigCommand => new CustomCommand(p => 
    {
        try
        {
            LoadingUI = true;
            var dialog = new SaveFileDialog
            {
                Title = "Zapisz plik",
                Filter = "XML (*.xml)|*.xml",
                DefaultExt = ".xml",
                FileName = "ModsConfig.xml"
            };
            if (dialog.ShowDialog() ?? false) _modsServices.SaveConfig(ModsConfigCollection, dialog.FileName);
        }
        catch (Exception ex) { }
        finally { LoadingUI = false; }
    });
    public CustomCommand SaveCommand => new CustomCommand(p => 
    {
        try
        {
            LoadingUI = true;
            if (ModsConfigCollection.Union(ModsCollection).Any())
            {
                _modsServices.SaveConfig(ModsConfigCollection);
                _modsServices.SaveLocalData(ModsConfigCollection.Union(ModsCollection));
            }
        }
        catch (Exception ex) { }
        finally { LoadingUI = false; }
    });

    public CustomCommand RimpyColorLoadCommand => new CustomCommand(p =>
    {
        try
        {
            LoadingUI = true;
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
        }
        catch (Exception ex) { }
        finally { LoadingUI = false; }
    });

    public CustomCommand ModsToCSVCommand => new CustomCommand(p =>
    {
        try
        {
            LoadingUI = true;
            var dialog = new SaveFileDialog
            {
                Title = "Zapisz plik",
                Filter = "CSV (*.csv)|*.csv",
                DefaultExt = ".xml",
                FileName = "Mods.csv"
            };
            if (dialog.ShowDialog() ?? false) _modsServices.ExportCSVMods(ModsConfigCollection.Union(ModsCollection), dialog.FileName);
        }
        catch (Exception ex) { }
        finally { LoadingUI = false; }
    });

    public CustomCommand ModDetailCommand => new CustomCommand(p =>
    {
        try
        {
            LoadingUI = true;
            new ModDetailWindow(this).ShowDialog();
        }
        catch (Exception ex) { }
        finally { LoadingUI = false; }
    });
    #endregion CustomCommand
}
using KeeperBaseLib.Model;
using KeeperBaseWPFLib.MVVM;
using Microsoft.Win32;
using RimKeeperModOrganizerLib.Models;
using RimKeeperModOrganizerLib.Services;
using RimKeeperModOrganizerWPF.Extensions;
using RimKeeperModOrganizerWPF.Views;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Media;
using RimKeeperModOrganizerLib.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Windows.Data;

namespace RimKeeperModOrganizerWPF.ViewModels;

public class MainViewModel : PropertyModel
{
    public ObservableCollection<string> ModGroups { get; set; } = new();
    public ObservableCollection<string> ModColors { get; set; } = new();
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
            }
        }
    }

    private readonly ModsServices _modsServices; 
    public MainViewModel(ModsServices modsServices)
    {
        _modsServices = modsServices;
        ModsConfigCollection.CollectionChanged += ModsConfigCollection_CollectionChanged;
        //LoadModsFromLocalAsync();

        //ModsConfigCollection = new CollectionViewSource { Source = AllMods }.View;
        //ModsConfigCollection.Filter = m => ((ModModel)m).Position >= 0;
        //ModsCollection = new CollectionViewSource { Source = AllMods }.View;
        //ModsCollection.Filter = m => ((ModModel)m).Position < 0;
    }
   // public ObservableCollection<ModModel> AllMods { get; } = new();
   // public ICollectionView ModsConfigCollection { get; }
   // public ICollectionView ModsCollection { get; }

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

    public bool LoadingUI = false;
    public bool IsUIListEnabled => !LoadingUI;
    public bool IsModsConfigAlert => ModsConfigCollection.Where(x => x.Alert != null).Any(x => x.Alert.Any());
    public Brush? GetModConfigStaticColor => IsModsConfigAlert ? Brushes.LightCoral : Brushes.Transparent;
    public string GetModConfigStaticLable => string.Format("Mods ({0}/{1})", ModsConfigCollection.Count, ModsCollection.Count);

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

    public CustomCommand OpenLinkCommand => new CustomCommand(p =>
    {
        if(p is string txt)
            Process.Start(new ProcessStartInfo
            {
                FileName = txt,
                UseShellExecute = true
            });
    });
    public CustomCommand OptionsCommand => new CustomCommand(p => App.Services.GetRequiredService<SettingsWindow>().ShowDialog());
    public CustomCommand ChangeColorCommand => new CustomCommand(p =>
    {
        if (SelectedMod == null) return;
       // new ChangeColorWindow(SelectedMod, ModColors).ShowDialog();
        new ChangeColorWindow(this).ShowDialog();
    });
    public CustomCommand RefreshCommand => new CustomCommand(p => LoadMods());
    public CustomCommand LoadConfigCommand => new CustomCommand(p => 
    {
        var dialog = new OpenFileDialog
        {
            Title = "Wybierz plik",
            Filter = "XML (*.xml)|*.xml|Wszystkie pliki (*.*)|*.*",
            Multiselect = false
        };
        if (dialog.ShowDialog()??false) LoadMods(dialog.FileName);
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
        if (dialog.ShowDialog()??false) _modsServices.SaveConfig(ModsConfigCollection, dialog.FileName);
    });
    public CustomCommand SaveCommand => new CustomCommand(p => 
    {
        try
        {
            LoadingUI = true;
            _modsServices.SaveConfig(ModsConfigCollection);
            _modsServices.SaveLocalData(ModsConfigCollection.Union(ModsCollection));

        }
        catch (Exception ex) { }
        finally { LoadingUI = false; }
    });
}
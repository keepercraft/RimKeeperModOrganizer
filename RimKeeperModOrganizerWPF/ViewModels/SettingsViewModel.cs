using KeeperBaseLib.Model;
using KeeperBaseWPFLib.MVVM;
using RimKeeperModOrganizerWPF.Models;
using RimKeeperModOrganizerWPF.Services;
namespace RimKeeperModOrganizerWPF.ViewModels;

public class SettingsViewModel : PropertyModel
{
    public SettingsModel Data {  get; set; } = new SettingsModel();

    private readonly SettingsService _settingsService;
    public SettingsViewModel() : this(App.SettingsService) { }
    public SettingsViewModel(SettingsService SettingsService)
    {
        _settingsService = SettingsService;
        _settingsService.CreateCopy(Data);
        Data.RaisePropertyChanged();
    }

    public event Action<bool?>? RequestClose;
    public void Close(bool save = false)
    {
        if (save)
        {
            _settingsService.ApplyChanges(Data);
            _settingsService.Save();
        }
        RequestClose?.Invoke(save);
    }
    public CustomCommand SaveCommand => new CustomCommand(p => Close(true));
    public CustomCommand CancelCommand => new CustomCommand(p => Close());
}
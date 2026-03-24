using KeeperBaseLib.Model;
using RimKeeperModOrganizerLib.Models;
using RimKeeperModOrganizerLib.Services;
using RimKeeperModOrganizerWPF.Views.Extensions;
namespace RimKeeperModOrganizerWPF.ViewModels;

public class SettingsViewModel : PropertyModel
{
    public SettingsModel Data {  get; set; } = new SettingsModel();
    public Dictionary<string, ColumnSettings> ModColumnData => Data.ModColumnData.ToDictionary(x => x.Key, x => x);
    private readonly SettingsService _settingsService;
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
            Data.RaisePropertyChanged();
            _settingsService.ApplyChanges(Data);
            _settingsService.Save();

        }
        RequestClose?.Invoke(save);
    }
    public CustomCommand SaveCommand => new CustomCommand(p => Close(true));
    public CustomCommand CancelCommand => new CustomCommand(p => Close());
}
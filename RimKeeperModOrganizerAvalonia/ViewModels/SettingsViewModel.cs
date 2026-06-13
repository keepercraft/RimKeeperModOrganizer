using KeeperBaseSharedLib.Models;
using KeeperBaseSheredLib;
using KeeperDataGridAvalonia.Extensions;
using RimKeeperModOrganizerAvalonia.Services;
using RimKeeperModOrganizerAvalonia.Views;
using RimKeeperModOrganizerLib.Helpers;
using RimKeeperModOrganizerLib.Models;
using RimKeeperModOrganizerLib.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace RimKeeperModOrganizerAvalonia.ViewModels;

public class SettingsViewModel : PropertyModel
{
    public SettingsModel Data { get; set; } = new SettingsModel();
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
            _settingsService.Settings.RaisePropertyChanged();
            _settingsService.Save();

        }
        RequestClose?.Invoke(save);
    }

    public string LocalDirectory => AppDomain.CurrentDomain.BaseDirectory;

    public CustomCommand SaveCommand => new CustomCommand(p => Close(true));
    public CustomCommand CancelCommand => new CustomCommand(p => Close());
    //public CustomCommand OpenLinkCommand => new CustomCommand(FileHelper.OpenLink);
    public CustomCommand OpenLinkCommand => new CustomCommand(p =>
    {
        if (p is not string propName) return;
        var prop = typeof(SettingsModel).GetProperty(propName);
        if (prop == null) return;
        var currentValue = prop.GetValue(Data) as string;
        if (string.IsNullOrEmpty(currentValue)) return;

        var t = AppDomain.CurrentDomain;
        FileHelper.OpenLink(currentValue);
    });
    public CustomCommand OpenFileCommand => new CustomCommand(p =>
    {
        if (p is not string propName) return;
        var prop = typeof(SettingsModel).GetProperty(propName);
        if (prop == null) return;
        var currentValue = prop.GetValue(Data) as string;
        if (string.IsNullOrEmpty(currentValue)) return;

        //new OpenFileDialog
        //{
        //    Title = propName,
        //    FileName = Path.GetFileName(currentValue),
        //    InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(currentValue))
        //}.ShowDialog((ok, path) =>
        //{
        //    if (!ok) return;
        //    prop.SetValue(Data, FileHelper.NormalizeBaseDirectoryPath(path));
        //    Data.RaisePropertyChanged(propName);
        //});
        StorageDialog.OpenFileDialog(
            title: propName,
            SuggestedFileName: Path.GetFileName(currentValue)??"",
            startLocation: Path.GetDirectoryName(Path.GetFullPath(currentValue))
        ).ContinueWith(t =>
        {
            var path = t.Result.FirstOrDefault();
            if (string.IsNullOrEmpty(path)) return;
            prop.SetValue(Data, FileHelper.NormalizeBaseDirectoryPath(path));
            Data.RaisePropertyChanged(propName);
        });
    });
    public CustomCommand OpenFolderCommand => new CustomCommand(p =>
    {
        if (p is not string propName) return;
        var prop = typeof(SettingsModel).GetProperty(propName);
        if (prop == null) return;
        var currentValue = prop.GetValue(Data) as string;
        if (string.IsNullOrEmpty(currentValue)) return;

        //var fds = new FolderDialogService();
        //fds.ShowDialog(propName, Path.GetFullPath(currentValue), (ok, path) =>
        //{
        //    if (!ok) return;
        //    prop.SetValue(Data, Path.GetDirectoryName(FileHelper.NormalizeBaseDirectoryPath(path)));
        //    Data.RaisePropertyChanged(propName);
        //});
        StorageDialog.OpenFolderDialog(
            title: propName,
            startLocation: Path.GetFullPath(currentValue)
        ).ContinueWith(t =>
        {
            var path = t.Result.FirstOrDefault();
            if (string.IsNullOrEmpty(path)) return;
            prop.SetValue(Data, Path.GetDirectoryName(FileHelper.NormalizeBaseDirectoryPath(path)));
            Data.RaisePropertyChanged(propName);
        });

        //new OpenFolderDialog
        //{
        //    Title = propName,
        //    InitialDirectory = Path.GetFullPath(currentValue)
        //}.ShowDialog((ok, path) =>
        //{
        //    if (!ok) return;
        //    prop.SetValue(Data, Path.GetDirectoryName(FileHelper.NormalizeBaseDirectoryPath(path)));
        //    Data.RaisePropertyChanged(propName);
        //});
    });
    public CustomCommand ToggleColumnsWidthCommand => new CustomCommand(() =>
    {
        if (WindowLocator.MainWindow is MainWindow mw)
        {
            mw?.ModsGrid?.ToggleStarColumns();
        }
    });
}



using Avalonia.Collections; // Zawiera DataGridCollectionView
using KeeperBaseSharedLib.Models;
using KeeperDataGridAvalonia.Models;
using KeeperDataGridAvaloniaExample.Models;
//using ReactiveUI; // Avalonia domyślnie korzysta z ReactiveUI do powiadomień
using System;
using System.Collections.ObjectModel;
namespace KeeperDataGridAvaloniaExample.ViewModels;

public class MainViewModel : PropertyModel //: ReactiveObject // Odpowiednik PropertyModel
{
    public MainViewModel()
    {
        Items = new ObservableCollection<TableModel>();
        LeftViewItems = new DataGridCollectionView(Items);
        LeftViewItems.Filter = LeftViewFilter;
        RightViewItems = new DataGridCollectionView(Items);
        RightViewItems.Filter = RightViewFilter;

        for (int i = 0; i < 10000; i++)
        {
            Items.Add(new TableModel() { Name = "test00" + i, Position = i, Age = (i * 2 + i) % 100 });
        }

        MyColumns.Add(new ColumnConfig() { PropertyName = nameof(TableModel.Name), Header = "BIGNAME", IsVisible=true, ShowFilter=true, ColumnIndex=6, Width="300" });
        MyColumns.Add(new ColumnConfig() { PropertyName = nameof(TableModel.Age), Header = "BIG AGE", IsVisible= true, ShowFilter=false, ColumnIndex=1, Width="*" });
    }

    public ObservableCollection<ColumnConfig> MyColumns { get; set; } = new();

    public ObservableCollection<TableModel> Items { get; set; }
    public IDataGridCollectionView LeftViewItems { get; }
    public IDataGridCollectionView RightViewItems { get; }
    public CustomCommand ViewRefreshCommand => new CustomCommand(p =>{
        LeftViewItems.Refresh();
        RightViewItems.Refresh();
    });
    private bool LeftViewFilter(object obj) => ((TableModel)obj)?.Position < 100;
    private bool RightViewFilter(object obj) => ((TableModel)obj)?.Position >= 100;


    //public string? FilterText { get; set; }

    private string? _filterText;
    public string? FilterText
    {
        get => _filterText;
        set
        {
            if (_filterText != value)
            {
                _filterText = value;
                OnPropertyChanged();
            }
            LeftViewItems.Refresh();
            RightViewItems.Refresh();
        }
    }

    //private bool LeftViewFilter(object obj)
    //{
    //    if (!string.IsNullOrEmpty(FilterText))
    //    {
    //        if (obj is TableModel tableItem2)
    //        {
    //            return tableItem2.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
    //        }
    //        return false;
    //    }
    //    if (obj is TableModel tableItem)
    //    {
    //        // Przykład logicznego filtra (Position >= 0)
    //        return tableItem.Position >= 0;
    //    }
    //    return false;
    //}

}
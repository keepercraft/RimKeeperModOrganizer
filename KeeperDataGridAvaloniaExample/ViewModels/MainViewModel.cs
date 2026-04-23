using Avalonia.Collections;
using KeeperBaseSharedLib.Models;
using KeeperBaseSheredLib;
using KeeperDataGridAvalonia.Models;
using KeeperDataGridAvaloniaExample.Helpers;
using KeeperDataGridAvaloniaExample.Models;
using System.Collections.ObjectModel;
using System.Linq;
namespace KeeperDataGridAvaloniaExample.ViewModels;

public class MainViewModel : PropertyModel
{
    public ObservableCollection<IColumnConfig> MyColumns { get; set; } = new();
    public ObservableCollection<TableModel> Items { get; set; }
    public IDataGridCollectionView LeftViewItems { get; }
    public IDataGridCollectionView RightViewItems { get; }
    private bool LeftViewFilter(object obj) => ((TableModel)obj)?.Position < 100;
    private bool RightViewFilter(object obj) => ((TableModel)obj)?.Position >= 100;

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

    public CustomCommand ViewRefreshCommand => new CustomCommand(Refresh);

    public MainViewModel()
    {
        Items = new ObservableCollection<TableModel>();
        LeftViewItems = new DataGridCollectionView(Items);
        LeftViewItems.Filter = LeftViewFilter;
        RightViewItems = new DataGridCollectionView(Items);
        RightViewItems.Filter = RightViewFilter;

        //for (int i = 0; i < 10000; i++)
        //{
        //    Items.Add(new TableModel() { Name = "test00" + i, Country = "", Position = i, Age = (i * 2 + i) % 100 });
        //}

        int i = 0;
        foreach (var item in DataGenHelper.DataGridTestData(1000))
        {
            item.Position = i++;
            Items.Add(item);
        }

        MyColumns.Add(new ColumnConfig() { PropertyName = nameof(TableModel.Name), Header = "BIG NAME", IsVisible=true, ShowFilter=true, ColumnIndex=6, Width="300" });
        MyColumns.Add(new ColumnConfig() { PropertyName = nameof(TableModel.Age), Header = "BIG AGE", IsVisible= true, ShowFilter=false, ColumnIndex=1, Width="*" });
        MyColumns.Add(new ColumnConfig() { PropertyName = nameof(TableModel.Country), Header = "SAMLL COUNTRY", IsVisible= true, ShowFilter= true, ColumnIndex=7, Width="200" });

        foreach (var item in MyColumns.Cast<ColumnConfig>())
        {
            item.PropertyChanged += (s, e) => Refresh();
        }
    }

    public void Refresh()
    {
        LeftViewItems.Refresh();
        RightViewItems.Refresh();
        RaisePropertyChanged(nameof(MyColumnsData));
    }

    public string MyColumnsData => string.Join(" || ", MyColumns.Select(c => $"{c.Header}:{c.ColumnIndex}:{c.Width}:{c.Filter}"));
}
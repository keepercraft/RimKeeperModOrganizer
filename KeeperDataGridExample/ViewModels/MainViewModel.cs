using GongSolutions.Wpf.DragDrop;
using KeeperBaseLib.Helper;
using KeeperBaseWPFLib.MVVM;
using KeeperDataGrid.Extensions;
using KeeperDataGrid.Models;
using KeeperDataGridExample.Helpers;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using PropertyModel = KeeperDataGrid.Models.PropertyModel;
namespace KeeperDataGridExample.ViewModels;

public class MainViewModel : PropertyModel, IDropTarget
{
    public ObservableCollection<ColumnConfig> MyColumns { get; } = new();

    public MainViewModel()
    {
        ViewItems = new ListCollectionView(Items); //CollectionViewSource.GetDefaultView(Items);
        ViewItems.CombineFilters([LeftViewFilter, UniversalFilter]);
        ViewPositionsItems = new ListCollectionView(Items); //CollectionViewSource.GetDefaultView(Items);
        ViewPositionsItems.CombineFilters(RightViewFilter);
        //ViewPositionsItems.SortDescriptions.Add(new SortDescription(nameof(TableModel.Position), ListSortDirection.Ascending));
        MyColumns.Add(new ColumnConfig() { PropertyName = nameof(TableModel.Position), Header = "P", Width = new DataGridLength(30), IsVisible=true, ShowFilter=false, ColumnIndex=1 });
        MyColumns.Add(new ColumnConfig() { PropertyName = nameof(TableModel.Name), Header = "Nazwa", Width = new DataGridLength(150), IsVisible=true });
        MyColumns.Add(new ColumnConfig() { PropertyName= nameof(TableModel.Age), Header = "Wiek", Width = new DataGridLength(100), IsVisible=true });
        MyColumns.Add(new ColumnConfig() { PropertyName= nameof(TableModel.Country), Header = "Kraj", Width = new DataGridLength(1, DataGridLengthUnitType.Star), IsVisible = true });




        Task.Run(() =>
        {
            int i = 0;
            foreach (var item in DataGenHelper.DataGridTestData(5))
            {
                item.Position = i++;
                App.Current.Dispatcher.Invoke(() => Items.Add(item));
            }
            foreach (var item in DataGenHelper.DataGridTestData(1000))
            {
                App.Current.Dispatcher.Invoke(() => Items.Add(item));
            }
        });

        //Columns.Add(new ColumnState { PropertyName = "Name", Width = 300 });
        //Columns.Add(new ColumnState { PropertyName = "Age", Width = 100 });
        //Columns.Add(new ColumnState { PropertyName = "Country", Width = 200 });
    }

    private string? _mutiSelectedItamText;
    public string? MutiSelectedItamText
    {
        get => _mutiSelectedItamText;
        set
        {
            _mutiSelectedItamText = value;
            OnPropertyChanged();
        }
    }
    public CustomCommand UpdateSelectionsCommand => new CustomCommand(p =>
    {
        var selectedItems = (p as IList)?.Cast<TableModel>();
        if (selectedItems == null || !selectedItems.Any())
            MutiSelectedItamText = string.Empty;
        else
            MutiSelectedItamText = string.Join(", ", selectedItems.Select(i => i.Name));
    });
    public CustomCommand ClearSortCommand => new CustomCommand(p => ViewItems.SortDescriptions.Clear());
    public CustomCommand ViewRefreshCommand => new CustomCommand(p => ViewItems?.Refresh());

    public ICollectionView ViewPositionsItems { get; }
    public ICollectionView ViewItems { get; }
    public ObservableCollection<TableModel> Items { get; set; } = new();
    public ObservableCollection<ColumnState> Columns { get; } = new();
   
    private TableModel? _selectedItam;
    public TableModel? SelectedItam
    {
        get => _selectedItam;
        set
        {
            if (_selectedItam != value)
            {
                _selectedItam = value;
                OnPropertyChanged();
            }
        }
    }

    private string? _filterText;
    public string? FilterText
    {
        get => _filterText;
        set
        {
            _filterText = value;
            OnPropertyChanged();
            ViewItems?.Refresh();
        }
    }

    private bool ModelNameFilter(object obj)
    {
        if(obj is TableModel model && !string.IsNullOrEmpty(FilterText))
            return model.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
        return true;
    }
    private bool LeftViewFilter(object obj) => ((TableModel)obj)?.Position == null;
    private bool RightViewFilter(object obj) => ((TableModel)obj)?.Position >= 0;
    private bool UniversalFilter(object obj)
    {
        if (obj == null) return false;
        if (string.IsNullOrWhiteSpace(FilterText)) return true;
        foreach (var prop in obj.GetType().GetProperties())
            if (prop.GetValue(obj)?.ToString()?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false)
                return true;
        return false;
    }
    


    private bool FilterOld(object obj)
    {
        if (Columns != null)
            foreach (var col in Columns)
            {
                var prop = obj.GetType().GetProperty(col.PropertyName)?.GetValue(obj);
                if (prop == null) continue;
                if (!string.IsNullOrWhiteSpace(col.FilterText))
                {
                    if (!prop.ToString().Contains(col.FilterText, StringComparison.OrdinalIgnoreCase)) return false;
                }
            }
        return true;
    }



    public void DragOver(IDropInfo dropInfo)
    {
        if (dropInfo.Data is TableModel || dropInfo.Data is IEnumerable<object>)
        {
            dropInfo.Effects = DragDropEffects.Move;
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
        }
    }
    public void Drop(IDropInfo dropInfo)
    {
        var itemsToMove = DefaultDropHandler.ExtractData(dropInfo.Data).Cast<TableModel>().Reverse();
        if (!itemsToMove.Any()) return;
        bool isDroppingToAssigned = dropInfo.TargetCollection == ViewPositionsItems;
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

        foreach (var item in Items.Where(w => w.Position != null))
        {
            item.Position = Items.IndexOf(item);
        }

        ViewItems.Refresh();
        ViewPositionsItems.Refresh();
    }
}

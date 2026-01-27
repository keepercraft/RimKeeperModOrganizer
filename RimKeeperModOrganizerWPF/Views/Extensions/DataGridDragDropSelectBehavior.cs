using RimKeeperModOrganizerLib.Models;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RimKeeperModOrganizerWPF.Views.Extensions;

public static class DataGridDragDropSelectBehavior
{
    private static Point _startPoint;
    private static DataGridRow _draggedRow;

    public static readonly DependencyProperty EnableRowDragDropProperty =
        DependencyProperty.RegisterAttached(
            "EnableRowDragDrop",
            typeof(bool),
            typeof(DataGridDragDropBehavior),
            new PropertyMetadata(false, OnEnableRowDragDropChanged));

    public static bool GetEnableRowDragDrop(DependencyObject obj)
        => (bool)obj.GetValue(EnableRowDragDropProperty);

    public static void SetEnableRowDragDrop(DependencyObject obj, bool value)
        => obj.SetValue(EnableRowDragDropProperty, value);

    private static void OnEnableRowDragDropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DataGrid grid)
        {
            if ((bool)e.NewValue)
            {
                grid.PreviewMouseLeftButtonDown += Grid_PreviewMouseLeftButtonDown;
                grid.PreviewMouseMove += Grid_PreviewMouseMove;
                grid.Drop += Grid_Drop;
                grid.AllowDrop = true;
            }
            else
            {
                grid.PreviewMouseLeftButtonDown -= Grid_PreviewMouseLeftButtonDown;
                grid.PreviewMouseMove -= Grid_PreviewMouseMove;
                grid.Drop -= Grid_Drop;
                grid.AllowDrop = false;
            }
        }
    }

    private static void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _startPoint = e.GetPosition(null);
        _draggedRow = FindVisualParent<DataGridRow>(e.OriginalSource as DependencyObject);
    }

    private static void Grid_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && _draggedRow != null)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = _startPoint - mousePos;

            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                var data = new DataObject();
                data.SetData("DataGridRow", _draggedRow.Item);       // element
                data.SetData("SourceDataGrid", sender);             // źródłowy DataGrid
                DragDrop.DoDragDrop(_draggedRow, data, DragDropEffects.Move);
            }
        }
    }

    private static void Grid_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent("DataGridRow") || !e.Data.GetDataPresent("SourceDataGrid"))
            return;

        var droppedItem = e.Data.GetData("DataGridRow") as ModModel;
        var sourceGrid = e.Data.GetData("SourceDataGrid") as DataGrid;
        var targetGrid = sender as DataGrid;

        if (droppedItem == null || sourceGrid == null || targetGrid == null) return;
        //if ((sourceGrid.ItemsSource is not IList sourceList || targetGrid.ItemsSource is not IList targetList)) return;
        if ((GetListFromItemsSource(sourceGrid.ItemsSource) is not IList sourceList || GetListFromItemsSource(targetGrid.ItemsSource) is not IList targetList)) return;

        var targetItem = GetTargetItem(targetGrid, e);

        // Obliczamy indeks
        int newIndex = targetItem != null ? targetList.IndexOf(targetItem) : targetList.Count;
        int oldIndex = sourceList.IndexOf(droppedItem);

        //  if (sourceList == targetList && oldIndex < newIndex)
        //   newIndex--; // uwzględniamy przesunięcie w tej samej liście



        bool sgc = sourceGrid.Name == "ModsGridConfig";
        bool tgc = targetGrid.Name == "ModsGridConfig";
        if (!sgc && !droppedItem.Selected) droppedItem.Position = targetItem?.Position ?? 0;
        if (!tgc && droppedItem.Selected) droppedItem.Position = -1;
        droppedItem.OnPropertyChanged(nameof(ModModel.Position));

        ((ObservableCollection<ModModel>)sourceList).Move(oldIndex, newIndex);

        //  sourceList.Remove(droppedItem);
        //  targetList.Insert(newIndex, droppedItem);
    }

    private static IList? GetListFromItemsSource(object? itemsSource)
    {
        return itemsSource switch
        {
            IList list => list,
            ICollectionView view => view.SourceCollection as IList,
            _ => null
        };
    }

    private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
    {
        DependencyObject parentObject = VisualTreeHelper.GetParent(child);
        if (parentObject == null) return null;
        if (parentObject is T parent) return parent;
        return FindVisualParent<T>(parentObject);
    }

    private static ModModel? GetTargetItem(DataGrid grid, DragEventArgs e)
    {
        DependencyObject depObj = e.OriginalSource as DependencyObject;
        while (depObj != null && depObj is not DataGridRow)
            depObj = VisualTreeHelper.GetParent(depObj);

        if (depObj is DataGridRow row && row.Item is ModModel target)
            return target;

        // Jeśli upuszczamy w puste miejsce, zwracamy ostatni element lub null
        if (grid.ItemsSource is IList list && list.Count > 0)
            return list[^1] as ModModel;

        return null;
    }

    private static void UpdatePositions(IList list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] is ModModel m)
                m.Position = i; // pamiętaj, że ModModel powinien implementować INotifyPropertyChanged
        }
    }
}

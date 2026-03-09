using FilterDataGrid;
using RimKeeperModOrganizerWPF.ViewModels;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace RimKeeperModOrganizerWPF;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // Loaded += MainWindow_Loaded;

      //  ModsGrid.LoadPreset();
      //  ModsGrid.SavePreset();
    }
/*
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            foreach (var col in ModsGrid.Columns)
            {
                if((col as dynamic)?.Binding is System.Windows.Data.Binding bind && bind != null) //FieldName
                {
                    string path = bind.Path.Path.ToLower();
                    var setting = vm.ModsCollectionColumnsData.FirstOrDefault(c => c.Name.ToLower() == path);
                    if (setting != null)
                    {
                        //col.Visibility = setting.Visible ? Visibility.Visible : Visibility.Collapsed;
                        var binding = new Binding(nameof(ColumnSettings.Visible))
                        {
                            Source = setting,
                            Mode = BindingMode.TwoWay,
                            Converter = new BooleanToVisibilityConverter()
                        };
                        BindingOperations.SetBinding(col, DataGridColumn.VisibilityProperty, binding);

                        //var widthBinding = new Binding(nameof(ColumnSettings.Width))
                        //{
                        //    Source = setting,
                        //    Mode = BindingMode.TwoWay
                        //};
                        //BindingOperations.SetBinding(col, DataGridColumn.WidthProperty, widthBinding);
                        //var widthBinding = new Binding(nameof(ColumnSettings.Width))
                        //{
                        //    Source = setting,
                        //    Mode = BindingMode.TwoWay
                        //}
                        //;
                        //BindingOperations.SetBinding(col, DataGridColumn.ActualWidthProperty, widthBinding);

                        //setting.PropertyChanged += (s, args) =>
                        //{
                        //    if (args.PropertyName == nameof(ColumnSettings.Width))
                        //    {
                        //        col.Width = setting.Width;
                        //    }
                        //};
                        //col.Width = setting.Width;
                    }
                }
            }
        }
    }
*/
    private void ModGroupsComboBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
        {
            var combo = sender as ComboBox;
            var vm = DataContext as MainViewModel;
            string text = combo?.Text?.Trim() ?? "";

            if (!string.IsNullOrEmpty(text) && !vm.ModGroups.Contains(text))
            {
                vm.ModGroups.Add(text);
                //vm.SelectedItem = text;
            }
            e.Handled = true; // aby nie zamknęło listy automatycznie
        }
    }

    private void MultiSelect_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if(e.OriginalSource is TextBox tb)
            {
                var context = MyMultiSelect;
                string t = tb.Text;
                if (!string.IsNullOrEmpty(t) && !context.ItemsSource.Contains(t))
                {
                    context.ItemsSource.Add(t);
                    context.SelectedItems.Add(t);
                    tb.Text = null;
                }
            }
        }
    }

    private void ModsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        ModsGrid.SavePreset();
        var a = GetGlobalFilterList(ModsGrid);
    }

    public static List<FilterCommon>? GetGlobalFilterList(object instance)
    {
        if (instance == null) return null;
        var prop = instance.GetType().GetProperty("GlobalFilterList", BindingFlags.Instance | BindingFlags.NonPublic);
        return prop?.GetValue(instance) as List<FilterCommon>;
    }
}
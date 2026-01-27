using RimKeeperModOrganizerWPF.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace RimKeeperModOrganizerWPF;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

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
}
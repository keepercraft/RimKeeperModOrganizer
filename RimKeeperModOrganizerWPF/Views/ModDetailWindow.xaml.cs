using RimKeeperModOrganizerWPF.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace RimKeeperModOrganizerWPF.Views;

public partial class ModDetailWindow : Window
{
    public ModDetailWindow(MainViewModel model)
    {
        InitializeComponent();
        DataContext = model;
        Title = model.SelectedMod?.About?.Name ?? "NO MOD";
    }

    private void MultiSelect_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (e.OriginalSource is TextBox tb)
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
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
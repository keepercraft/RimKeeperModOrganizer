using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.VisualTree;
using System.Collections.ObjectModel;
using System.Linq;
namespace RimKeeperModOrganizerAvalonia.Controls;

public partial class TagAutoCompleteBox : UserControl
{
    public static readonly StyledProperty<ObservableCollection<string>?> AvailableTagsProperty =
        AvaloniaProperty.Register<TagAutoCompleteBox, ObservableCollection<string>?>(nameof(AvailableTags));

    public static readonly StyledProperty<ObservableCollection<string>?> SelectedTagsProperty =
        AvaloniaProperty.Register<TagAutoCompleteBox, ObservableCollection<string>?>(nameof(SelectedTags));

    public ObservableCollection<string>? AvailableTags
    {
        get => GetValue(AvailableTagsProperty);
        set => SetValue(AvailableTagsProperty, value);
    }

    public ObservableCollection<string>? SelectedTags
    {
        get => GetValue(SelectedTagsProperty);
        set => SetValue(SelectedTagsProperty, value);
    }

    public TagAutoCompleteBox()
    {
        InitializeComponent();

        var ac = this.FindControl<AutoCompleteBox>("Part_AutoComplete");
        ac.Loaded += (s, e) =>
        {
            var tb = ac.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();
            tb?.KeyDown += (sender, e) =>
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        //ac.IsDropDownOpen = false;
                        e.Handled = AddTag(tb);
                        break;
                    case Key.Up or Key.Down:
                        ac.IsDropDownOpen = true;
                        e.Handled = true;
                        ac.Focus();
                        break;
                }
            };
            ac?.KeyUp += (sender, e) =>
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        e.Handled = AddTag(tb);
                        break;
                }
            };
            ac?.PointerReleased += (sender, e) =>
            {
                if (e.Source is ContentPresenter accc)
                    e.Handled = AddTag(tb);
                else if (!ac.IsDropDownOpen && (AvailableTags?.Any() ?? false))
                    ac.IsDropDownOpen = true;
            };
        };
    }
    private bool AddTag(TextBox tb)
    {
        AddTag(tb.Text);
        tb.Text = string.Empty;
        return true;
    }
    private void AddTag(string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return;
        var cleanTag = tag.Trim();

        if (SelectedTags != null && !SelectedTags.Contains(cleanTag))
        {
            SelectedTags.Add(cleanTag);
        }
        if (AvailableTags != null && !AvailableTags.Contains(cleanTag))
        {
            AvailableTags.Add(cleanTag);
        }
    }

    private void RemoveTag_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string tag })
        {
            SelectedTags?.Remove(tag);
        }
    }
}
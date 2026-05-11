using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Input;
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

        // Szukamy wewnętrznego TextBoxa po załadowaniu kontrolki
        var ac = this.FindControl<AutoCompleteBox>("Part_AutoComplete");
        ac.Loaded += (s, e) =>
        {
            var tb = ac.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();
            if (tb != null)
            {
                tb.KeyDown += (sender, args) =>
                {
                    if (args.Key == Key.Enter)
                    {
                        AddTag(tb.Text);
                        tb.Text = string.Empty;
                        ac.IsDropDownOpen = false;
                        args.Handled = true;
                    }
                };
            }
        };
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

    private void AutoCompleteBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is string selected)
        {
            AddTag(selected);
            // Czyścimy tekst po wyborze z listy
            var ac = sender as AutoCompleteBox;
            if (ac != null)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    ac.SelectedItem = null;
                    ac.Text = string.Empty;
                });
            }
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
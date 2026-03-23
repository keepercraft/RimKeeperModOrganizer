using System.ComponentModel;
namespace KeeperDataGrid.Models;

public class ColumnState : PropertyModel
{
    public event Action? FilterChanged;

    public string PropertyName { get; set; }

    public ListSortDirection? SortDirection { get; set; }

    public bool IsVisible { get; set; } = true;

    public double Width { get; set; }

    public int DisplayIndex { get; set; }

    private string _filterText;
    public string FilterText
    {
        get => _filterText;
        set
        {
            if (_filterText == value) return;
            _filterText = value;
            OnPropertyChanged();
            FilterChanged?.Invoke();
        }
    }
    public object? FilterValue1 { get; set; }  // np. min lub pierwszy wybór
    public object? FilterValue2 { get; set; }  // np. max lub drugi wybór
    public FilterType Type { get; set; } = FilterType.Text;
}

public enum FilterType
{
    Text,
    NumberRange,
    DateRange,
    Boolean,
    Custom
}
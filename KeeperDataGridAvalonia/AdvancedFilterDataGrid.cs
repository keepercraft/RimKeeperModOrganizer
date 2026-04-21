using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using KeeperDataGridAvalonia.Extensions;
namespace KeeperDataGridAvalonia;

public class AdvancedFilterDataGrid : DataGridTextColumn
{
    public string Key { get; set; } = string.Empty;
    private static readonly DataGridLengthConverter _lengthConverter = new();

    public AdvancedFilterDataGrid()
    {
        Width = new DataGridLength(1, DataGridLengthUnitType.Star);
        ColumnIndexProperty.Changed.AddClassHandler<AdvancedFilterDataGrid>((x, e) => x.OnColumnsColumnIndexChanged(e));
    }
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == FilterValueProperty)
        {
            if (OwningGrid?.ItemsSource is DataGridCollectionView view)
            {
                view.Refresh();
            }
        }
        else if (change.Property == WidthTextProperty)
        {
            OnWidthTextChanged(change.GetNewValue<string?>());
        }
        else if (change.Property == WidthProperty)
        {
            OnWidthChanged(change.GetNewValue<DataGridLength>());
        }
    }

    #region ColumnIndex
    public static readonly StyledProperty<int?> ColumnIndexProperty 
        = AvaloniaProperty.Register<AdvancedFilterDataGrid, int?>(nameof(ColumnIndex), defaultValue: null);
    public int? ColumnIndex
    {
        get => GetValue(ColumnIndexProperty);
        set => SetValue(ColumnIndexProperty, value);
    }
    private void OnColumnsColumnIndexChanged(AvaloniaPropertyChangedEventArgs e)
    {
        OwningGrid?.Columns
            .OrderBy(s => s is AdvancedFilterDataGrid adv ? adv.ColumnIndex ?? -1 : s.DisplayIndex)
            .Select((s, i) => s.DisplayIndex = i)
            .ToList();
    }
    #endregion

    #region WIDTH
    public static readonly StyledProperty<string?> WidthTextProperty =
        AvaloniaProperty.Register<AdvancedFilterDataGrid, string?>(nameof(WidthText));
    public string? WidthText
    {
        get => GetValue(WidthTextProperty);
        set => SetValue(WidthTextProperty, value);
    }

    private bool _isInternalUpdate;

    private void OnWidthTextChanged(string? newStr)
    {
        if (_isInternalUpdate || string.IsNullOrWhiteSpace(newStr)) return;
        if (newStr.Equals("NaN", StringComparison.OrdinalIgnoreCase)) return;
        try
        {
            var length = (DataGridLength?)_lengthConverter.ConvertFromInvariantString(newStr);
            if (length.HasValue)
            {
                _isInternalUpdate = true;
                Width = length.Value;
                _isInternalUpdate = false;
            }
        }
        catch { }
    }

    private void OnWidthChanged(DataGridLength currentWidth)
    {
        if (_isInternalUpdate) return;
        if (currentWidth.IsAbsolute && double.IsNaN(currentWidth.Value)) return;
        string newWidthText = currentWidth.ToXamlString();
        if (WidthText != newWidthText)
        {
            _isInternalUpdate = true;
            WidthText = newWidthText;
            _isInternalUpdate = false;
        }
    }
    #endregion

    #region FILTER / SELECTBOX
    public static readonly StyledProperty<bool> ShowFilterProperty =
        AvaloniaProperty.Register<AdvancedFilterDataGrid, bool>(nameof(ShowFilter), true);
    public bool ShowFilter
    {
        get => GetValue(ShowFilterProperty);
        set => SetValue(ShowFilterProperty, value);
    }

    public static readonly StyledProperty<string?> FilterValueProperty =
        AvaloniaProperty.Register<AdvancedFilterDataGrid, string?>(nameof(FilterValue));
    public string? FilterValue
    {
        get => GetValue(FilterValueProperty);
        set => SetValue(FilterValueProperty, value);
    }

    public static readonly StyledProperty<bool> ShowSelectBoxFilterProperty =
        AvaloniaProperty.Register<AdvancedFilterDataGrid, bool>(nameof(ShowSelectBoxFilter), false);
    public bool ShowSelectBoxFilter
    {
        get => GetValue(ShowSelectBoxFilterProperty);
        set => SetValue(ShowSelectBoxFilterProperty, value);
    }

    public static readonly StyledProperty<IEnumerable<string>?> SelectBoxFilterListProperty =
        AvaloniaProperty.Register<AdvancedFilterDataGrid, IEnumerable<string>?>(nameof(SelectBoxFilterList));
    public IEnumerable<string>? SelectBoxFilterList
    {
        get => GetValue(SelectBoxFilterListProperty);
        set => SetValue(SelectBoxFilterListProperty, value);
    }
    #endregion

    public object? GetRowValue(object rowItem)
    {
        if (string.IsNullOrEmpty(Key) || rowItem == null) return null;
        return rowItem.GetType().GetProperty(Key)?.GetValue(rowItem);
    }
}
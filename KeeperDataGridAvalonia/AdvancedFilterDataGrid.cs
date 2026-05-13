using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.Templates;
using KeeperDataGridAvalonia.Extensions;
using System.Diagnostics;
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
        //else if (change.Property == ShowFilterProperty)
        //{
        if (ShowSelectBoxFilter && SelectBoxFilterList != null && FilterValue == null)
        {
            ShowFilter = false;
            foreach (var item in SelectBoxFilterList)
            {
                if (string.IsNullOrEmpty(item))
                {
                    FilterValue = item;
                    break;
                }
            }
        }
        //}
        //var property = change.Property.Name;
        //var newvalue = change.NewValue;
        //Debug.WriteLine("AdvancedFilterDataGrid: " + property + " -> " + newvalue);
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
            Debug.WriteLine("AdvancedFilterDataGrid:OnWidthTextChanged: " + this.Key + " v" + newStr.ToString());
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
        Debug.WriteLine("AdvancedFilterDataGrid:OnWidthChanged: " + this.Key
            + " v"       + currentWidth.Value 
            + " display" + currentWidth.DisplayValue
            + " desire"  + currentWidth.DesiredValue);

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

    public static readonly StyledProperty<DataTemplate> SelectBoxItemTemplateProperty =
        AvaloniaProperty.Register<AdvancedFilterDataGrid, DataTemplate>(nameof(SelectBoxItemTemplate), null);
    public DataTemplate SelectBoxItemTemplate
    {
        get => GetValue(SelectBoxItemTemplateProperty);
        set => SetValue(SelectBoxItemTemplateProperty, value);
    }

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
        var path = GetPropertyNameFromColumn(this);
        if (string.IsNullOrEmpty(path) || rowItem == null) return null;
        object? current = rowItem;
        foreach (var part in path.Split('.'))
        {
            if (current == null) return null;
            var prop = current.GetType().GetProperty(part);
            if (prop == null) return null;
            current = prop.GetValue(current);
        }
        return current;
    }

    public string? GetPropertyNameFromColumn(DataGridTextColumn column)
    {
        var binding = column.Binding;
        if (binding is CompiledBindingExtension compiled)
        {
            return compiled.Path?.ToString();
        }
        if (binding is Binding standardBinding)
        {
            return standardBinding.Path;
        }
        if (binding is CompiledBinding cBinding)
        {
            return cBinding.Path.ToString();
        }
        return null;
    }
}
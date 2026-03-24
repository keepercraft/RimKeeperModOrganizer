using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
namespace KeeperDataGrid;

public class FilterableTextColumn : DataGridTextColumn
{
    public FilterableTextColumn()
    {
        Width = new DataGridLength(1, DataGridLengthUnitType.Star);
        WidthTextCallback();
    }
    public FilterableTextColumn(string key, Style? style = null)
    {
        Width = new DataGridLength(1, DataGridLengthUnitType.Star);
        Key = key;
        Header = key; // Przepisujemy nazwę właściwości jako nagłówek
        Binding = new Binding(key) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
        WidthTextCallback();
       // if (style != null) CellStyle = style;
    }

    public string Key { get; set; } = string.Empty;

    public int? ColumnIndex
    {
        get => (int?)GetValue(ColumnIndexProperty);
        set => SetValue(ColumnIndexProperty, value);
    }
    public static readonly DependencyProperty ColumnIndexProperty =
        DependencyProperty.Register(nameof(ColumnIndex), typeof(int?), typeof(FilterableTextColumn),
            new PropertyMetadata(null));

    #region  WIDTH
    public string? WidthText
    {
        get => (string?)GetValue(WidthTextProperty);
        set => SetValue(WidthTextProperty, value);
    }
    public static readonly DependencyProperty WidthTextProperty =
        DependencyProperty.Register(nameof(WidthText), typeof(string), typeof(FilterableTextColumn),
            new PropertyMetadata(null, OnWidthTextChanged));
    private static void OnWidthTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var column = (FilterableTextColumn)d;
        if (column._isInternalUpdate) return;

        var newStr = e.NewValue as string;
        if (string.IsNullOrWhiteSpace(newStr) || newStr.Equals("NaN", StringComparison.OrdinalIgnoreCase)) return;

        try
        {
            var cvt = new DataGridLengthConverter();
            var length = (DataGridLength)cvt.ConvertFromInvariantString(newStr);
            // Podwójne sprawdzenie przed przypisaniem do kolumny
            if (length.IsAbsolute && (double.IsNaN(length.Value) || length.Value < 0)) return;
            column._isInternalUpdate = true;
            column.Width = length;
            column._isInternalUpdate = false;
        }
        catch
        {
            // Ignoruj błędy parsowania
        }
    }

    private bool _isInternalUpdate;
    private void WidthTextCallback()
    {
        var dpd = DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn));
        dpd.AddValueChanged(this, (s, e) =>
        {
            if (_isInternalUpdate) return; // Jeśli zmiana przyszła z modelu, nie wysyłaj jej z powrotem
            var currentWidth = this.Width;
            // OCHRONA: Jeśli szerokość jest w stanie nieustalonym (NaN), nie synchronizuj jej
            if (currentWidth.IsAbsolute && double.IsNaN(currentWidth.Value)) return;
            if (currentWidth.IsStar && double.IsNaN(currentWidth.Value)) return;
            var cvt = new DataGridLengthConverter();
            string currentWidthStr = cvt.ConvertToInvariantString(currentWidth);
            // Dodatkowe sprawdzenie, czy konwerter nie wypluł dosłownego tekstu "NaN"
            if (currentWidthStr.Contains("NaN", StringComparison.OrdinalIgnoreCase)) return;
            if (WidthText != currentWidthStr)
            {
                _isInternalUpdate = true;
                WidthText = currentWidthStr;
                _isInternalUpdate = false;
            }
        });
    }
    #endregion

    #region FILTER
    public Visibility ShowFilter
    {
        get => (Visibility)GetValue(ShowFilterProperty);
        set => SetValue(ShowFilterProperty, value);
    }
    public static readonly DependencyProperty ShowFilterProperty =
        DependencyProperty.Register(nameof(ShowFilter), typeof(Visibility), typeof(FilterableTextColumn),
            new PropertyMetadata(Visibility.Visible));

    public string FilterValue
    {
        get => (string)GetValue(FilterValueProperty);
        set => SetValue(FilterValueProperty, value);
    }
    public static readonly DependencyProperty FilterValueProperty =
        DependencyProperty.Register(nameof(FilterValue), typeof(string), typeof(FilterableTextColumn),
            new PropertyMetadata(null, OnFilterChanged));

    private static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FilterableTextColumn column)
        {
            var dataGrid = column.DataGridOwner;
            if (dataGrid != null && dataGrid.ItemsSource != null)
            {
                ICollectionView view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
                view?.Refresh();
            }
        }
    }

    private readonly FrameworkElement _evaluator = new FrameworkElement();
    private bool _isBindingInitialized = false;
    private static readonly DependencyProperty ValueEvaluatorProperty =
        DependencyProperty.Register("ValueEvaluator", typeof(object), typeof(FilterableTextColumn));
    public object GetRowValue(object rowItem)
    {
        if (!_isBindingInitialized)
        {
            if (Binding is Binding binding)
            {
                BindingOperations.SetBinding(_evaluator, ValueEvaluatorProperty, binding);
            }
            _isBindingInitialized = true;
        }
        _evaluator.DataContext = rowItem;
        return _evaluator.GetValue(ValueEvaluatorProperty);
    }
    #endregion
}
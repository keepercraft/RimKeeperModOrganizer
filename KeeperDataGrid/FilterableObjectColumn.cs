using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
namespace KeeperDataGrid;

public class FilterableObjectColumn : DataGridTemplateColumn
{
    public FilterableObjectColumn()
    {
        Width = new DataGridLength(1, DataGridLengthUnitType.Star);
        WidthTextCallback();
    }
    public FilterableObjectColumn(string key, Style? style = null)
    {
        Width = new DataGridLength(1, DataGridLengthUnitType.Star);
        Key = key;
        Header = key; // Przepisujemy nazwę właściwości jako nagłówek
       // Binding = new Binding(key) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
        WidthTextCallback();
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
        DependencyProperty.Register(nameof(WidthText), typeof(string), typeof(FilterableObjectColumn),
            new PropertyMetadata(null, OnWidthTextChanged));
    private static void OnWidthTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var column = (FilterableObjectColumn)d;
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
    #endregion
}
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
    }
    public FilterableTextColumn(string key, Style? style = null)
    {
        Width = new DataGridLength(1, DataGridLengthUnitType.Star);
        Key = key;
        Header = key; // Przepisujemy nazwę właściwości jako nagłówek
        Binding = new Binding(key) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
       // if (style != null) CellStyle = style;
    }

    public string Key { get; set; } = string.Empty;

    public Visibility ShowFilter
    {
        get => (Visibility)GetValue(ShowFilterProperty);
        set => SetValue(ShowFilterProperty, value);
    }
    public static readonly DependencyProperty ShowFilterProperty =
        DependencyProperty.Register(nameof(ShowFilter), typeof(Visibility), typeof(FilterableTextColumn),
            new PropertyMetadata(Visibility.Visible));

    public int? ColumnIndex
    {
        get => (int?)GetValue(ColumnIndexProperty);
        set => SetValue(ColumnIndexProperty, value);
    }
    public static readonly DependencyProperty ColumnIndexProperty =
        DependencyProperty.Register(nameof(ColumnIndex), typeof(int?), typeof(FilterableTextColumn),
            new PropertyMetadata(null));

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

    // Ukryty obiekt pomocniczy (tylko jeden na kolumnę!)
    private readonly FrameworkElement _evaluator = new FrameworkElement();
    private bool _isBindingInitialized = false;

    // Rejestrujemy właściwość, do której bindowanie będzie wrzucać wynik
    private static readonly DependencyProperty ValueEvaluatorProperty =
        DependencyProperty.Register("ValueEvaluator", typeof(object), typeof(FilterableTextColumn));

    public object GetRowValue(object rowItem)
    {
        // Inicjalizacja bindowania tylko przy pierwszym użyciu
        if (!_isBindingInitialized)
        {
            if (Binding is Binding binding)
            {
                // Klonujemy bindowanie, ale NIE ustawiamy Source. 
                // Będzie ono korzystać z DataContextu obiektu _evaluator.
                BindingOperations.SetBinding(_evaluator, ValueEvaluatorProperty, binding);
            }
            _isBindingInitialized = true;
        }

        // KLUCZ DO WYDAJNOŚCI: Zmieniamy tylko kontekst danych
        _evaluator.DataContext = rowItem;

        // Pobieramy gotowy wynik (WPF już go przeliczył przy zmianie DataContextu)
        return _evaluator.GetValue(ValueEvaluatorProperty);
    }
}


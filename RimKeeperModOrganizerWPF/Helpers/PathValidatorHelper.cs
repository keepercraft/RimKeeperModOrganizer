using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace RimKeeperModOrganizerWPF.Helpers;

public static class PathValidatorHelper
{
    public static readonly DependencyProperty ValidateProperty =
           DependencyProperty.RegisterAttached(
               "Validate",
               typeof(bool),
               typeof(PathValidatorHelper),
               new PropertyMetadata(false, OnValidateChanged));

    private static readonly DependencyProperty TokenProperty =
        DependencyProperty.RegisterAttached(
            "Token",
            typeof(CancellationTokenSource),
            typeof(PathValidatorHelper));

    public static void SetValidate(DependencyObject element, bool value)
        => element.SetValue(ValidateProperty, value);

    public static bool GetValidate(DependencyObject element)
        => (bool)element.GetValue(ValidateProperty);

    private static void OnValidateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox tb)
            return;

        if ((bool)e.NewValue)
            tb.TextChanged += TextChanged;
        else
            tb.TextChanged -= TextChanged;
    }

    private static void TextChanged(object sender, TextChangedEventArgs e)
    {
        var tb = (TextBox)sender;

        var oldToken = (CancellationTokenSource)tb.GetValue(TokenProperty);
        oldToken?.Cancel();

        var cts = new CancellationTokenSource();
        tb.SetValue(TokenProperty, cts);

        _ = ValidateAsync(tb, cts.Token);
    }

    private static async Task ValidateAsync(TextBox tb, CancellationToken token)
    {
        try
        {
            await Task.Delay(200, token); // debounce

            var path = tb.Dispatcher.Invoke(() => tb.Text);

            bool exists = false;

            if (!string.IsNullOrWhiteSpace(path))
                exists = await Task.Run(() => File.Exists(path) || Directory.Exists(path), token);

            tb.Dispatcher.Invoke(() =>
            {
                if (exists)
                {
                    tb.BorderBrush = Brushes.LightGreen;
                    tb.BorderThickness = new Thickness(2);
                }
                else
                {
                    tb.BorderBrush = Brushes.LightCoral;
                    tb.BorderThickness = new Thickness(2);
                }
            });
        }
        catch (TaskCanceledException)
        {
        }
    }
}

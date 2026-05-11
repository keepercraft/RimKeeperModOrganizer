using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
namespace RimKeeperModOrganizerAvalonia.Behaviors;

public class PathValidatorBehavior
{
    public static readonly AttachedProperty<bool> ValidateProperty =
        AvaloniaProperty.RegisterAttached<
            PathValidatorBehavior,
            AvaloniaObject,
            bool>(
                "Validate",
                false);

    private static readonly AttachedProperty<CancellationTokenSource?> TokenProperty =
        AvaloniaProperty.RegisterAttached<
            PathValidatorBehavior,
            AvaloniaObject,
            CancellationTokenSource?>(
                "Token");

    static PathValidatorBehavior()
    {
        ValidateProperty.Changed.AddClassHandler<TextBox>(OnValidateChanged);
    }

    public static void SetValidate(AvaloniaObject element, bool value)
        => element.SetValue(ValidateProperty, value);

    public static bool GetValidate(AvaloniaObject element)
        => element.GetValue(ValidateProperty);

    private static void OnValidateChanged(TextBox tb, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not bool enabled)
            return;

        if (enabled)
            tb.TextChanged += TextChanged;
        else
            tb.TextChanged -= TextChanged;
    }

    private static void TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox tb)
            return;

        var oldToken = tb.GetValue(TokenProperty);

        oldToken?.Cancel();
        oldToken?.Dispose();

        var cts = new CancellationTokenSource();

        tb.SetValue(TokenProperty, cts);

        _ = ValidateAsync(tb, cts.Token);
    }

    private static async Task ValidateAsync(TextBox tb, CancellationToken token)
    {
        try
        {
            await Task.Delay(200, token);

            string path = tb.Text ?? "";

            bool exists = false;

            if (!string.IsNullOrWhiteSpace(path))
            {
                exists = await Task.Run(() =>
                    File.Exists(path) || Directory.Exists(path),
                    token);
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                tb.BorderBrush = exists ? Brushes.LightGreen : Brushes.Red;
                tb.BorderThickness = new Thickness(2);
                //tb.Classes.Set("path-valid", exists);
                //tb.Classes.Set("path-invalid", !exists);
            });
        }
        catch (TaskCanceledException)
        {
        }
    }
}
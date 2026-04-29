using Avalonia;
namespace RimKeeperModOrganizerAvalonia.Extensions;

public class BindingProxy : AvaloniaObject
{
    // Definicja StyledProperty (odpowiednik DependencyProperty)
    public static readonly StyledProperty<object?> DataProperty = AvaloniaProperty.Register<BindingProxy, object?>(nameof(Data));
    public object? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }
}
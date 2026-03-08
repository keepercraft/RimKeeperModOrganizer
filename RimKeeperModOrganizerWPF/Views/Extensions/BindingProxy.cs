using System.Windows;
namespace RimKeeperModOrganizerWPF.Views.Extensions;

public class BindingProxy : Freezable
{
    protected override Freezable CreateInstanceCore() => new BindingProxy();
    public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new PropertyMetadata(null));
    public object Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }
}
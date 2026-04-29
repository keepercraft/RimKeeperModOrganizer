using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
namespace RimKeeperModOrganizerAvalonia.Behaviors;

public static class WindowPositionBinding
{
    public static readonly AttachedProperty<PixelPoint> ProxyPositionProperty =
        AvaloniaProperty.RegisterAttached<Window, PixelPoint>(
            "ProxyPosition",
            typeof(WindowPositionBinding),
            defaultBindingMode: BindingMode.TwoWay);

    public static void SetProxyPosition(Window element, PixelPoint value) => element.SetValue(ProxyPositionProperty, value);
    public static PixelPoint GetProxyPosition(Window element) => element.GetValue(ProxyPositionProperty);

    public static readonly AttachedProperty<bool> TrackPositionProperty =
        AvaloniaProperty.RegisterAttached<Window, bool>(
            "TrackPosition",
            typeof(WindowPositionBinding));

    public static void SetTrackPosition(Window element, bool value) => element.SetValue(TrackPositionProperty, value);
    public static bool GetTrackPosition(Window element) => element.GetValue(TrackPositionProperty);

    static WindowPositionBinding()
    {
        ProxyPositionProperty.Changed.AddClassHandler<Window>((window, e) =>
        {
            if (e.NewValue is PixelPoint newPos && window.Position != newPos)
            {
                window.Position = newPos;
            }
        });

        TrackPositionProperty.Changed.AddClassHandler<Window>((window, e) =>
        {
            if (e.NewValue is bool isTracking && isTracking)
            {
                window.PositionChanged += (sender, args) =>
                {
                    var currentBound = GetProxyPosition(window);
                    if (currentBound != args.Point)
                    {
                        SetProxyPosition(window, args.Point);
                    }
                };
            }
        });
    }
}
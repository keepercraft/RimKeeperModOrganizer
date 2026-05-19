using Avalonia;
using Avalonia.Controls;
namespace KeeperDataGridAvalonia.Behaviors;

public static class DragAndDropBehavior
{
    public static readonly AttachedProperty<bool> BlockGridDragProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>(
            "BlockGridDrag",
            typeof(DragAndDropBehavior));

    static DragAndDropBehavior()
    {
        BlockGridDragProperty.Changed.AddClassHandler<Control>((ctrl, e) =>
        {
            if ((bool)e.NewValue!)
            {
                ctrl.PointerPressed += (_, ev) =>
                {
                    ev.Handled = true;
                };
            }
        });
    }

    public static void SetBlockGridDrag(AvaloniaObject element, bool value)
        => element.SetValue(BlockGridDragProperty, value);

    public static bool GetBlockGridDrag(AvaloniaObject element)
        => element.GetValue(BlockGridDragProperty);
}
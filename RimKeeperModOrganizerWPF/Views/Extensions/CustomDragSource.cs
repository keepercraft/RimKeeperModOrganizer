using GongSolutions.Wpf.DragDrop;
using System.Windows.Input;
namespace RimKeeperModOrganizerWPF.Views.Extensions;

public class CustomDragSource : DefaultDragHandler
{
    public override bool CanStartDrag(IDragInfo dragInfo)
    {
        return !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
    }
}
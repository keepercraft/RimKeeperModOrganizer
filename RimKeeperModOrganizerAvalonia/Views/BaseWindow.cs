using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
namespace RimKeeperModOrganizerAvalonia.Views;

public class BaseWindow : Window
{
    public BaseWindow()
    {
        ExtendClientAreaToDecorationsHint = true;
        ExtendClientAreaTitleBarHeightHint = -1;
    }

    // DRAG OKNA
    public void OnTitleBarPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    // MINIMIZE
    public void MinimizeClick(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    // MAXIMIZE
    public void MaximizeClick(object? sender, RoutedEventArgs e)
    {
        WindowState =
            WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
    }

    // CLOSE
    public void CloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
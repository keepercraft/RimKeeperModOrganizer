using System;
using System.Windows.Input;
namespace KeeperDataGridAvaloniaExample.Models;
public class CustomCommand : ICommand
{
    private readonly Func<object?, bool> _canExecute;
    private readonly Action<object?> _execute;

    // W Avalonii musisz ręcznie wywołać to zdarzenie, 
    // jeśli wynik CanExecute ulegnie zmianie.
    public event EventHandler? CanExecuteChanged;

    public CustomCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute ?? (_ => true);
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute(parameter);
    }

    public void Execute(object? parameter)
    {
        _execute(parameter);
    }

    // Metoda pomocnicza, którą wywołujesz w ViewModelu, 
    // gdy chcesz odświeżyć stan przycisku (Enabled/Disabled)
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

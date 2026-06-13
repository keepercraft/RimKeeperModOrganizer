using System.Diagnostics;
using System.Windows.Input;
namespace KeeperBaseSheredLib;

public class CustomCommand : ICommand
{
    private readonly Func<object?, bool> _canExecute;
    private readonly Action<object?> _execute;
    public event EventHandler? CanExecuteChanged;

    public CustomCommand(Func<object?, object?> execute)
    {
        _execute = _ => execute(_);
        _canExecute = _ => true;
    }
    public CustomCommand(Action execute)
    {
        _execute = _ => execute();
        _canExecute = _ => true;
    }
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

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
using System;
using System.Windows.Input;

namespace KeeperBaseWPFLib.MVVM
{
    public class CustomCommand : ICommand
    {
        Func<object, bool> _CanExecureDelegate = null;
        public Func<object, bool> CanExecureDelegate
        {
            get { return _CanExecureDelegate; }
            set { _CanExecureDelegate = value; }
        }

        Action<object> _ExecuteDelegate = null;
        public Action<object> ExecuteDelegate
        {
            get { return _ExecuteDelegate; }
            set { _ExecuteDelegate = value; }
        }

        public CustomCommand(Func<object, bool> InCanExecureDelegate, Action<object> InExecuteDelegate)
        {
            _CanExecureDelegate = InCanExecureDelegate;
            _ExecuteDelegate = InExecuteDelegate;
        }

        public CustomCommand(Action<object> InExecuteDelegate)
        {
            _CanExecureDelegate = (a) => { return true; };
            _ExecuteDelegate = InExecuteDelegate;
        }

        public bool CanExecute(object parameter)
        {
            return CanExecureDelegate(parameter);
        }

        public void Execute(object parameter = null)
        {
            ExecuteDelegate(parameter);
        }

        public void EventHandle(object sender, object parameter)
        {
            ExecuteDelegate(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}

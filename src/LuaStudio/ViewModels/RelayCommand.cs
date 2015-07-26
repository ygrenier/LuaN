using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace LuaStudio.ViewModels
{

    public class RelayCommand : ICommand
    {
        Action _Execute;
        Func<bool> _CanExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            if (execute == null) throw new ArgumentNullException("execute");
            this._Execute = execute;
            this._CanExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_CanExecute != null)
                return _CanExecute();
            return true;
        }

        public void Execute(object parameter)
        {
            _Execute();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

    }

    public class RelayCommand<T> : ICommand
    {
        Action<T> _Execute;
        Func<T, bool> _CanExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            if (execute == null) throw new ArgumentNullException("execute");
            this._Execute = execute;
            this._CanExecute = canExecute;
        }
        bool ICommand.CanExecute(object parameter)
        {
            T p = default(T);
            if (parameter is T) p = (T)parameter;
            return CanExecute(p);
        }
        public bool CanExecute(T parameter)
        {
            if (_CanExecute != null)
                return _CanExecute(parameter);
            return true;
        }
        void ICommand.Execute(object parameter)
        {
            T p = default(T);
            if (parameter is T) p = (T)parameter;
            Execute(p);
        }
        public void Execute(T parameter)
        {
            _Execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

    }

}

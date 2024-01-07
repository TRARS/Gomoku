using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Gomoku.Core.Helper.Base
{
    //public class RelayCommand : ICommand
    //{
    //    readonly Action<object?> _execute;
    //    readonly Func<object?, bool> _canExecute;

    //    public RelayCommand(Action<object?> execute) : this(execute, null!) { }
    //    public RelayCommand(Action<object?> execute, Func<object?, bool> canExecute)
    //    {
    //        if (execute == null)
    //            throw new ArgumentNullException("execute");
    //        _execute = execute; _canExecute = canExecute;
    //    }

    //    public bool CanExecute(object? parameter)
    //    {
    //        return _canExecute == null ? true : _canExecute(parameter);
    //    }
    //    public event EventHandler? CanExecuteChanged
    //    {
    //        add { CommandManager.RequerySuggested += value; }
    //        remove { CommandManager.RequerySuggested -= value; }
    //    }
    //    public void Execute(object? parameter) { _execute(parameter); }
    //}

    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object?, Task> execute;
        private readonly Func<object?, bool> canExecute;

        private long isExecuting;

        public AsyncRelayCommand(Func<object?, Task>? execute, Func<object?, bool>? canExecute = null)
        {
            this.execute = execute ?? (async _ => await Task.Yield());
            this.canExecute = canExecute ?? (_ => true);
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        public bool CanExecute(object? parameter)
        {
            if (Interlocked.Read(ref isExecuting) != 0)
                return false;

            return canExecute(parameter);
        }

        public async void Execute(object? parameter)
        {
            Interlocked.Exchange(ref isExecuting, 1);
            RaiseCanExecuteChanged();

            try
            {
                await execute(parameter);
            }
            finally
            {
                Interlocked.Exchange(ref isExecuting, 0);
                RaiseCanExecuteChanged();
            }
        }
    }
}

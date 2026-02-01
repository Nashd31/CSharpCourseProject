using System.Windows.Input;

namespace Telhai.DotNet.PlayerProject
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        public RelayCommand(Action execute)
        {
            _execute = execute;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _execute();
        }

        public event EventHandler? CanExecuteChanged;
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        public RelayCommand(Action<T> execute)
        {
            _execute = execute;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _execute((T)parameter!);
        }

        public event EventHandler? CanExecuteChanged;
    }
}
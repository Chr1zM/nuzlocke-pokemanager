using System.Windows.Input;

namespace PokeManager.PokeManagement
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute_;
        private readonly Predicate<object> canExecute_;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            execute_ = execute;
            this.canExecute_ = canExecute;
        }

        public bool CanExecute(object parameter) => canExecute_?.Invoke(parameter) ?? true;
        public void Execute(object parameter) => execute_(parameter);
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}

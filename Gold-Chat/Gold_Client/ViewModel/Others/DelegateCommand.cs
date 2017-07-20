using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Gold_Client.ViewModel.Others
{
    public class DelegateCommand : ICommand
    {
        private readonly Action _action;
        private readonly Action<object> actionObject;
        private readonly Predicate<Object> _predicate;

        public DelegateCommand(Action<Object> action) : this(action, null)
        {
        }

        public DelegateCommand(Action<Object> action, Predicate<Object> predicate)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action), @"You must specify an Action<T>.");
            }
            actionObject = action;
            _predicate = predicate;
        }

        public DelegateCommand(Action action)
        {
            _action = action;
        }

        public void Execute(object parameter)
        {
            _action?.Invoke();
            actionObject?.Invoke(parameter);
        }

        public bool CanExecute(object parameter)
        {
            if (_predicate == null)
            {
                return true;
            }
            return _predicate(parameter);
        }

        public void Execute()
        {
            Execute(null);
        }

        public bool CanExecuteUpdateTextBoxBindingOnEnterCommand(object parameter)
        {
            return true;
        }

        public void ExecuteUpdateTextBoxBindingOnEnterCommand(object parameter)
        {
            TextBox tBox = parameter as TextBox;
            if (tBox != null)
            {
                DependencyProperty prop = TextBox.TextProperty;
                BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
                if (binding != null)
                    binding.UpdateSource();
            }
        }

#pragma warning disable 67
        public event EventHandler CanExecuteChanged { add { } remove { } }
#pragma warning restore 67
    }
}

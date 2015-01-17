using System;

namespace Barak.Benchmark.Presentation.ViewModels
{
    public class RelayCommand : System.Windows.Input.ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Func<object, bool> m_onCanExecute = e => { return true; };
        private Action<object> m_onExecute;

        public RelayCommand(Action<object> onExecute)
        {
            m_onExecute = onExecute;
        }

        public RelayCommand(Action<object> onExecute, Func<object, bool> onCanExecute)
            : this(onExecute)
        {
            m_onCanExecute = onCanExecute;
        }
        public bool CanExecute(object parameter)
        {
            return m_onCanExecute.Invoke(parameter);
        }

        public void Execute(object parameter)
        {
            m_onExecute.Invoke(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            var temp = CanExecuteChanged;
            if (temp != null)
            {
                temp.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Barak.Benchmark.Presentation.ViewModels
{
    public class BenchmarkNodeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool? m_selected;
        private string m_description;
        public ObservableCollection<BenchmarkNodeViewModel> Children { get; protected set; }

        public string Id { get; set; }
        public BenchmarkNodeViewModel Parent { get; set; }
        public System.Windows.Input.ICommand DeleteCommand { get; private set; } 
        public ICommand RenameCommand { get; protected set; }

        
        public bool? Selected
        {
            get
            {
                return m_selected;
            }
            set
            {
                if (m_selected != value)
                {
                    m_selected = value;
                    OnPropertyChanged("Selected");
                    OnSelectionChanged(value);
                }
            }
        }

        public string Description
        {
            get
            {
                return m_description;
            }
            set
            {
                if (m_description != value)
                {
                    m_description = value;
                    OnPropertyChanged("Description");
                }
            }
        }


        public BenchmarkNodeViewModel(System.Windows.Input.ICommand deleteCommand,System.Windows.Input.ICommand renameCommand)
        {
            RenameCommand = renameCommand;
            Children = new ObservableCollection<BenchmarkNodeViewModel>();
            Selected = false;
            ((INotifyPropertyChanged)Children).PropertyChanged += Children_PropertyChanged;
            DeleteCommand = deleteCommand;
        }

        void Children_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateSelection();
        }

        private void OnSelectionChanged(bool? propertyChanged)
        {
            if (propertyChanged.HasValue)
            {
                foreach (var benchmarkNodeViewModel in Children)
                {
                    benchmarkNodeViewModel.Selected = propertyChanged.Value;
                }   
            }

            UpdateSelection();
        }

        private void UpdateSelection()
        {
            if (Parent != null)
            {
                if (Parent.Children.All(p => p.Selected == true))
                {
                    Parent.Selected = true;
                }
                else if (Parent.Children.All(p => p.Selected == false))
                {
                    Parent.Selected = false;
                }
                else if (Parent.Children.Any(p => p.Selected == true || p.Selected == null))
                {
                    Parent.Selected = null;
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

using Common;
using Nito.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Chapter14_Wpf
{
    /// <summary>
    /// Interaction logic for Chapter14_03.xaml
    /// </summary>
    public partial class Chapter14_03 : Window, IChapter
    {


        public Chapter14_03()
        {
            // DataBinding requires that a value is required immediately. If the
            // data comes from an asynchronous source, the NotifyTask will show a
            // default value first and then provide the value from the
            // asynchronous source when it arrives.
            InitializeComponent();
            DataContext = this;
            MyValue = NotifyTask.Create(CalculateMyValueAsync());
            MyBindableTask = new BindableTask<int>(CalculateMyValueAsync());
        }

        public void Run()
        {
            Show();
        }

        public NotifyTask<int> MyValue { get; set; }
        public BindableTask<int> MyBindableTask { get; set; }

        private async Task<int> CalculateMyValueAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            return 13;
        }
    }

    public class BindableTask<T> : INotifyPropertyChanged
    {
        private readonly Task<T> _task;

        public BindableTask(Task<T> task)
        {
            _task = task;
            var _ = WatchTaskAsync();
        }

        // This method gets called first and the task begins and is awaited
        private async Task WatchTaskAsync()
        {
            try
            {
                // ConfigureAwait(false) is not used because the property changed
                // event should be raised on the UI thread 
                await _task;
            }
            catch
            {
                // This empty catch is here on purpose as the code wants to
                // handle errors using data binding instead of here.
            }

            // The properties are first read before these events are raised
            // once the task completes the task state will have changed and these
            // PropertyChanged events are raised allowing the UI to read the current
            // state of the task, either Faulted or RanToCompletion

            OnPropertyChanged("IsNotCompleted");
            OnPropertyChanged("IsSuccessfullyCompleted");
            OnPropertyChanged("IsFaulted");
            OnPropertyChanged("Result");
        }

        // After the constructor calls the WatchTaskAsync method these properties
        // will all be read, while the task itself is still executing, returning
        // the current state of the task. Each one of these properties is tied to a 
        // different label on the UI. Only one is true at a time. The Result property
        // will first return a default value, but it is not shown on screen because
        // the task won't have completed Once the task has completed the
        // OnProperty changed event fires allowing the UI to be updated
        public bool IsNotCompleted
        {
            get
            {
                return !_task.IsCompleted;
            }
        }

        public bool IsSuccessfullyCompleted
        {
            get
            {
                return _task.Status == TaskStatus.RanToCompletion;
            }
        }

        public bool IsFaulted
        {
            get
            { 
                return _task.IsFaulted; 
            }
        }

        public T Result
        {
            get 
            { 
                return IsSuccessfullyCompleted ? _task.Result : default; 
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

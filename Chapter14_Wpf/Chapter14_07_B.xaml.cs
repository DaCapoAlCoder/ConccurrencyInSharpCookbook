using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for Chapter14_07_B.xaml
    /// </summary>
    public partial class Chapter14_07_B : Window
    {
        public Chapter14_07_B()
        {
            InitializeComponent();
            DataContext = this;
        }
        private string Solve(IProgress<int> progress)
        {
            // Count as quickly as possible for 3 seconds.
            var endTime = DateTime.UtcNow.AddSeconds(3);
            int value = 0;
            while (DateTime.UtcNow < endTime)
            {
                // This code runs on a background thread and is still counting
                // as fast as possible. The difference now is the events raised
                // by calling the Report method are being converted to an observable
                // stream. The observable is only picking up on values from the stream
                // at a given time interval because it was configured with the Sample
                // method. Throttling is therefore handled in the just before sending
                // to the UI thread and code can execute as intended. There is also
                // separation of concerns as not all users of this method would want
                // the same kind of throttling
                value++;
                progress?.Report(value);
            }
            return value.ToString();
        }

        public static class ObservableProgress
        {
            // This is a custom implementation of IProgress<T> used in
            // place of the default Progress<T> class
            private sealed class EventProgress<T> : IProgress<T>
            {
                // This is just an explicit implementation of IProgress<T>
                // For each progress report it simply invokes an event instead
                // of calling the delegate the default implementation takes
                void IProgress<T>.Report(T value) => OnReport?.Invoke(value);
                public event Action<T> OnReport;
            }

            public static (IObservable<T>, IProgress<T>) Create<T>()
            {
                var progress = new EventProgress<T>();

                // This code creates an observable from the OnReport event
                // defined in the EventProgress class above. This adds
                // the handler to the OnReport event which is raised 
                // every time Report is called on the Customer IProgress<T>
                // implementation. The code causes the observable to
                // receive the OnReport events by wrapping them
                var observable = Observable.FromEvent<T>(
                    handler => progress.OnReport += handler,
                    handler => progress.OnReport -= handler);
                return (observable, progress);
            }

            // Note: this must be called from the UI thread.
            public static (IObservable<T>, IProgress<T>) CreateForUi<T>(TimeSpan? sampleInterval = null)
            {
                // This creates both the IProgress<T> instance and the observable configured to
                // pass a handler into the Report event
                var (observable, progress) = Create<T>();
                // The observable is configured to fire at a given time interval, this is 
                // throttling the updates. The observable is also being told to operate on
                // the UI thread here. However sample might operate on a different thread allowing
                // throttling to happen before the delegate is called on the UI thread (possibly)
                observable = observable.Sample(sampleInterval ?? TimeSpan.FromMilliseconds(100))
                    .ObserveOn(SynchronizationContext.Current);
                return (observable, progress);
            }
        }

        // For simplicity, this code updates a label directly.
        // In a real-world MVVM application, those assignments
        //  would instead be updating a ViewModel property
        //  which is data-bound to the actual UI.
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            MyLabel.Content = "Starting...";
            var (observable, progress) = ObservableProgress.CreateForUi<int>();
            string result;
            // Subscribing here passes the delegate into the observable. The
            // observable then connects the delegate as a handler to the Report
            // event raised by the customer IProgress<T> implementation. This
            // creates an observable stream using the events raised by the 
            // custom IProgress<T> implementation which are throttled before hitting
            // the UI thread by only observing (sampling) at a given interval.
            using (observable.Subscribe(value => MyLabel.Content = value))
            {
                result = await Task.Run(() => Solve(progress));
            }
            MyLabel.Content = $"Done! Result: {result}";
        }
    }
}

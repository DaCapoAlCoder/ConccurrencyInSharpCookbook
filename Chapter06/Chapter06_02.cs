using Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Chapter06
{
    public class Chapter06_02 : IChapter
    {
        public void Run()
        {
            ObserverWillExecuteOnDifferentThreads();
            ForceObserverToExecuteOnASyncContext();

            Win win = new();
            win.Show();
            win.CaptureMouseData();
        }


        public void ObserverWillExecuteOnDifferentThreads()
        {
            // The subscriber will execute its code in any available thread
            // It can run on different threads each time it executes
            SynchronizationContext uiContext = SynchronizationContext.Current;
            Trace.WriteLine($"UI thread is {Environment.CurrentManagedThreadId}");
            Observable.Interval(TimeSpan.FromSeconds(1))
                .Take(5)
                .Subscribe(x => Trace.WriteLine($"Interval {x} on thread {Environment.CurrentManagedThreadId}"));
        }

        public void ForceObserverToExecuteOnASyncContext()
        {
            //SynchronizationContext uiContext = SynchronizationContext.Current;
            Trace.WriteLine($"UI thread is {Environment.CurrentManagedThreadId}");
            Observable.Interval(TimeSpan.FromSeconds(1))
                .Take(5)
                // Add the ObserveOn method to specify which context to run on
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(x => Trace.WriteLine($"Interval {x} on thread {Environment.CurrentManagedThreadId}"));

        }

        class Win : Window
        {
            public void CaptureMouseData()
            {
                SynchronizationContext uiContext = SynchronizationContext.Current;
                Trace.WriteLine($"UI thread is {Environment.CurrentManagedThreadId}");
                Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(
                        handler => (s, a) => handler(s, a),
                        handler => MouseMove += handler,
                        handler => MouseMove -= handler)
                    .Select(evt => evt.EventArgs.GetPosition(this))
                    // Using the default scheduler moves the observations to the thread pool
                    // i.e. the default scheduler
                    .ObserveOn(Scheduler.Default)
                    .Select(position =>
                    {
                        // These operations now happening in any thread thread from the thread pool
                        // Complex calculation
                        Thread.Sleep(100);
                        var result = position.X + position.Y;
                        Trace.WriteLine($"Calculated result {result} on thread {Environment.CurrentManagedThreadId}");
                        return result;
                    })
                    // The second observe on switches the result of the above operations back to the ui thread
                    // synchronization might get tricky here
                    .ObserveOn(uiContext)
                    .Subscribe(x => Trace.WriteLine($"Result {x} on thread {Environment.CurrentManagedThreadId}"));
            }

        }
    }
}

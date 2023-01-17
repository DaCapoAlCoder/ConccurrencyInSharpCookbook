using Common;
using System;
using System.Diagnostics;
using System.Net;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace Chapter06
{
    public class Chapter06_01 : IChapterAsync
    {
        public async Task Run()
        {
            var progress = CreateStreamFromProgressEvents();
            await FakePercentComplete(progress);

            await EventDelegatePerType();
            await EventDelegatePerTypeUsingReflection();
            ErrorsInObservableWrappedEvents();
        }
        Progress<int> CreateStreamFromProgressEvents()
        {
            var progress = new Progress<int>();
            // From event pattern uses the EventArgs<T> which is common in newer UI frameworks
            IObservable<EventPattern<int>> progressReports =
                Observable.FromEventPattern<int>(
                    // This adds/removes the observable event handler from the progress event
                    handler => progress.ProgressChanged += handler,
                    handler => progress.ProgressChanged -= handler);

            // Subscribes to the observable stream and writes out the output
            progressReports.Subscribe(data => Trace.WriteLine("OnNext: " + data.EventArgs + "%"));

            // This is just an event handler copied across for the progress event
            // its not required
            //progress.ProgressChanged += (sender, args) =>
            //{
            //    Trace.WriteLine($"Complete: {args}%");
            //};
            return progress;
        }
        async Task FakePercentComplete(IProgress<int> progress = null)
        {
            bool done = false;
            double percentComplete = 0;
            int i = 0;
            const int max = 3;
            while (!done)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                percentComplete = (i + 1) / (double)max;
                int wholePercentComplete = (int)(percentComplete * 100);
                progress?.Report(wholePercentComplete);
                done = ++i >= 3;
            }
        }
        async Task EventDelegatePerType()
        {
            // Older Event frameworks defined their own event delegate per event
            var timer = new System.Timers.Timer(interval: 1000) { Enabled = true };

            IObservable<EventPattern<ElapsedEventArgs>> ticks =
                // This is typed to the event type and the event args type
                Observable.FromEventPattern<ElapsedEventHandler, ElapsedEventArgs>(

                    // This maps the ElapsedTimeEventHandler to EventHandler
                    handler => (s, a) => handler(s, a),
                    handler => timer.Elapsed += handler,
                    handler => timer.Elapsed -= handler);

            // Dispose the subscription after a few seconds so the program doesn't run forever
            var disposable = ticks.Subscribe(data => Trace.WriteLine("OnNext: " + data.EventArgs.SignalTime));
            await Task.Delay(TimeSpan.FromSeconds(5));
            disposable.Dispose();
        }


        async Task EventDelegatePerTypeUsingReflection()
        {
            var timer = new System.Timers.Timer(interval: 1000) { Enabled = true };

            // Everything above applies its just a little neater using reflection
            IObservable<EventPattern<object>> ticks =
                Observable.FromEventPattern(timer, nameof(System.Timers.Timer.Elapsed));

            // The input type is object because reflection was used, generics above allow
            // the type to be inferred
            var disposable = ticks.Subscribe(data => Trace.WriteLine("OnNext: "
                // Note the cast here that is not used in the other implementations
                + ((ElapsedEventArgs)data.EventArgs).SignalTime));
            await Task.Delay(TimeSpan.FromSeconds(5));
            disposable.Dispose();
        }

        void ErrorsInObservableWrappedEvents()
        {
            var client = new WebClient();

            // When wrapping an event in an observable errors are just passed through as data
            // and not an error
            IObservable<EventPattern<object>> downloadedStrings =
                Observable.FromEventPattern(client, nameof(WebClient.DownloadStringCompleted));

            try
            {
                downloadedStrings.Subscribe(
            data =>
            {
                // Has the usual three reactive components next, error, complete
                var eventArgs = (DownloadStringCompletedEventArgs)data.EventArgs;
                if (eventArgs.Error != null)
                    Trace.WriteLine("OnNext: (Error) " + eventArgs.Error);
                else
                    Trace.WriteLine("OnNext: " + eventArgs.Result);
            },
            ex => Trace.WriteLine("OnError: " + ex.ToString()),
            () => Trace.WriteLine("OnCompleted"));
                client.DownloadStringAsync(new Uri("http://invalid.example.com/"));

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
    }
}

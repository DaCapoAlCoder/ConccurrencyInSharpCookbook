using Common;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter10
{
    public class Chapter10_06 : IChapterAsync
    {
        public async Task Run()
        {
            // The first two examples are basically the same thing. Convert
            // the observable to a task adding the token as an argument the
            // conversion method
            using CancellationTokenSource cts = new();
            var token = cts.Token;
            var task = ReturnLastElement(token);
            cts.Cancel();
            try
            {
                await task;

            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Observable stream as task cancelled");
            }

            using CancellationTokenSource cts2 = new();
            token = cts2.Token;
            var task2 = ReturnAllElements(token);
            cts2.Cancel();
            try
            {
                await task2;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Observable stream returning all elements as task cancelled");
            }

            // Using Cancellation Disposable will cancel an operation when disposed
            // rather than directly calling cancel
            CancellationDisposableTest(true);
            CancellationDisposableTest(false);
        }

        Task<int> ReturnLastElement(CancellationToken cancellationToken)
        {
            IObservable<int> observable = Observable.Range(0, 10);
            return observable
                .Delay(TimeSpan.FromSeconds(2)).
                TakeLast(1)
                .ToTask(cancellationToken);
        }

        Task<IList<int>> ReturnAllElements(CancellationToken cancellationToken)
        {
            IObservable<int> observable = Observable.Range(0, 10); // ...
            return observable
                .Delay(TimeSpan.FromSeconds(1))
                .ToList()
                .ToTask(cancellationToken);
        }

        void CancellationDisposableTest(bool waitForObservable)
        {

            ManualResetEvent manualResetEvent = new(!waitForObservable);

            CancellationToken tokenOutside = default;
            using (var cancellation = new CancellationDisposable())
            {
                CancellationToken token = cancellation.Token;
                // Pass the token to methods that respond to it.
                tokenOutside = token;

                HttpClient httpClient = new();

                Observable.FromAsync((token) => httpClient.GetStringAsync("https://www.example.com", token))
                    .Subscribe(x =>
                   {
                        Console.WriteLine(x);
                        manualResetEvent.Set();
                    });

                if (waitForObservable)
                {
                    WaitHandle.WaitAny(new WaitHandle[] { manualResetEvent });
                }
                Console.WriteLine($"Inside of disposable area cancellation requested: {token.IsCancellationRequested}");
            }
            // At this point, the token is cancelled.

            Console.WriteLine($"Outside of disposable area cancellation requested: {tokenOutside.IsCancellationRequested}");
        }
    }
}

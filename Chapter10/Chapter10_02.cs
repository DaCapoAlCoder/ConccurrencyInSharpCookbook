using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter10
{
    public class Chapter10_02 : IChapterAsync
    {
        public async Task Run()
        {
            // Creating a method that can be cancelled when the work being 
            // carried out does not itself have a cancellation API

            // Note that CancellationTokenSources are disposable
            using CancellationTokenSource cts = new();
            var task1 = Task.Run(() => CancelableMethod(cts.Token));
            await Task.Delay(TimeSpan.FromSeconds(3));
            cts.Cancel();
            try
            {
                await task1;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was cancelled");
            }

            using CancellationTokenSource cts2 = new();
            task1 = Task.Run(() => CancelableMethodFastLoop(cts2.Token));
            await Task.Delay(TimeSpan.FromSeconds(3));
            cts2.Cancel();
            try
            {
                await task1;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was cancelled");
            }
        }

        public int CancelableMethod(CancellationToken cancellationToken)
        {
            // Cancel a slow running loop by polling the cancellation token for cancellation
            // on each iterations
            for (int i = 0; i != 100; ++i)
            {
                Thread.Sleep(1000); // Some calculation goes here.
                Console.WriteLine($"iteration: {i}");
                // This is another way to access if cancellation was requested, its possible to return null
                // or default if this property returns true. This is not the standard cancellation behaviour
                // however and so should be avoided with preference to throw if cancellation is requested. If
                // this approach is taken in spite of that, then it should at least be documented

                Console.WriteLine($"Was cancellation requested: {cancellationToken.IsCancellationRequested}");
                cancellationToken.ThrowIfCancellationRequested();
            }
            return 42;
        }

        public int CancelableMethodFastLoop(CancellationToken cancellationToken)
        {
            // If the loop runs fast then it might be better to pull on only a
            // certain number of iterations. The performance should be measured
            // before and after making this kind of optimisation to check if it
            // actually makes a difference or just makes performance worse
            for (int i = 0; i != 100000; ++i)
            {
                Thread.Sleep(1); // Some calculation goes here.
                if (i % 100 == 0)
                {
                    Console.WriteLine($"iteration: {i}");
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            return 42;
        }
    }
}

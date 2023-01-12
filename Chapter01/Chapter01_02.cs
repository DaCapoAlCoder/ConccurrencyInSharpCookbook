using Common;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter01
{
    public class Chapter01_02 : IChapter
    {
        public void Run()
        {
            // It is better to use continue await false in core library methods and
            // only resume context on the outer "UI" methods

            // Create a context for the method to execute in
            AsyncContext.Run(async () =>
            {
                // ConfigureAwait is used to prevent the default behaviour for
                // an async method of resuming on the original context
                await DoSomethingAsync();
            });

            // Await can also be used with ValueTask<T> value tasks can have a memory
            // improvement if the result is usually synchronous, such as fetching the
            // value from a local cache instead of making a database call to get it. A
            // ValueTask isn't directly convertible to Task but it can be awaited. Most
            // times await is used with Task or Task<T>
        }

        async Task DoSomethingAsync()
        {
            // This will resume on the context because configure await is true
            int value = 13;
            Console.WriteLine($"Synchronous section before configure await true: {Thread.CurrentThread.ManagedThreadId}");
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(true);
            Console.WriteLine($"Synchronous section after configure await true: {Thread.CurrentThread.ManagedThreadId}\n");

            // The following two methods might run on different or the same thread, but that thread won't be the original
            // context used to execute the first method. These methods will resume on a thread pool thread.

            // This will not resume on the context because configure await is false
            // Asynchronously wait 1 second.
            Console.WriteLine($"Synchronous section before configure await false: {Thread.CurrentThread.ManagedThreadId}");
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            Console.WriteLine($"Synchronous section after configure await false: {Thread.CurrentThread.ManagedThreadId}\n");

            value *= 2;

            // This will not resume on the context because configure await is false
            // Asynchronously wait 1 second.
            Console.WriteLine($"Synchronous section before configure await false: {Thread.CurrentThread.ManagedThreadId}");
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            Console.WriteLine($"Synchronous section after configure await false: {Thread.CurrentThread.ManagedThreadId}\n");

            Console.WriteLine(value);
        }
    }
}

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
    public class Chapter01_05 : IChapter
    {
        public void Run()
        {
            // This demonstration shows that once async is used it is best to
            // use it through the full call stack and eventually await it somewhere.
            // The main problem here is the Deadlock method is synchronous and does
            // not await but instead uses the Wait method to synchronously wait on the
            // task to complete

            Deadlock();
            Console.WriteLine("Outside of context this won't deadlock");

            // A context like a UI context or an old ASP.Net framework context will
            // only allow one thread in at a time. The AsyncContext here acts in a similar
            // manner.
            AsyncContext.Run(() =>
            {
                Deadlock();
            });
            Console.WriteLine("Not Deadlocked");
        }

        async Task WaitAsync()
        {
            // This await will capture the current context ...
            Console.WriteLine($"Inside WaitAsync before the method thread ID:{Thread.CurrentThread.ManagedThreadId}");
            await Task.Delay(TimeSpan.FromSeconds(1));
            Console.WriteLine($"Inside WaitAsync after the method thread ID:{Thread.CurrentThread.ManagedThreadId}");
            // ... and will attempt to resume the method here in that context.
        }

        void Deadlock()
        {
            // This method runs inside a context that will only allow one thread in at
            // a time. The WaitMethod returns a task that will take one second to complete
            // due to the delay. While the asynchronous task is working, this method continues
            // and begins to wait synchronously for the task to complete. This then blocks
            // the context thread. Meanwhile the task completes and is also trying to
            // resume on the same context thread, but it can't because the wait method
            // is blocking it. This results in a deadlock.

            // In a console application the context is a thread pool. If the calling thread
            // is blocked by the Wait method, the thread pool will simply resume the async
            // method on another thread, this is why this code won't deadlock in a console
            // application. Typically a different thread Id will be written out after the
            // delay is awaited when called from a thread pool context. 

            // Start the delay.
            Console.WriteLine($"Before calling WaitAsync method thread ID: {Thread.CurrentThread.ManagedThreadId}");
            Task task = WaitAsync();

            // Synchronously block, waiting for the async method to complete.
            Console.WriteLine($"About to call wait on the task thread ID: {Thread.CurrentThread.ManagedThreadId}");
            task.Wait();
            Console.WriteLine($"Successfully waited for the task to complete synchronously thread ID: {Thread.CurrentThread.ManagedThreadId}");
        }
    }
}

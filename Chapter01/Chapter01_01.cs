using Common;
using System;
using System.Threading.Tasks;

namespace Chapter01
{
    public class Chapter01_01 : IChapterAsync
    {
        public async Task Run()
        {
            // Asynchronous methods will try to resume on the current context
            // in a console application like this, the context is the thread
            // pool. On resuming the method after an await, the method will
            // continue executing on any thread pool thread. For a UI application
            // the context could be the UI thread, if that's where the method is called,
            // and the method will try to resume there. Likewise an ASP.Net framework app
            // there is a request context that the method will resume on.
            await DoSomethingAsync();
        }

        // Never return void from an async method
        async Task DoSomethingAsync()
        {
            // Synchronous portion
            int value = 13;

            // This section checks if the Task is already complete. If it
            // is, it will continue synchronously. If it is not it pause the
            // method and return an incomplete task. When the asynchronous 
            // operation is complete at some later stage the method will
            // continue
            // Asynchronously wait 1 second.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Synchronous portion
            value *= 2;

            // Asynchronously wait 1 second.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Synchronous portion
            Console.WriteLine(value);
        }
    }
}

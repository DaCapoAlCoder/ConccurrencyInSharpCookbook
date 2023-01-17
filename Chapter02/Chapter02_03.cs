using Common;
using System;
using System.Threading.Tasks;

namespace Chapter02
{
    public class Chapter02_03 : IChapterAsync
    {
        public async Task Run()
        {
            await CallMyMethodAsync();
        }

        async Task MyMethodAsync(IProgress<double> progress = null)
        {
            bool done = false;
            double percentComplete = 0;
            int i = 0;
            const int max = 3;
            while (!done)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                percentComplete = (i + 1) / (double)max;
                progress?.Report(percentComplete);
                done = ++i >= 3;
            }
        }

        async Task CallMyMethodAsync()
        {
            // Progress<T> uses the current synchronization context if there is one available. This allows updates to be sent to a UI thread.
            // The synchronization context of the UI will keep updates synchronized. The synchronization context can keep the execution of
            // the progress updates serialized. The synchronization context allows the continuation after an asynchronous method to return
            // to the same thread.
            // But if there is no current synchronization context a thread from the thread pool is used. 
            // Executing fast updates that execute events handlers on the thread pool can cause the same event handlers to run on different threads at
            // the same time. This can result in multiple threads updating the same piece of state data, causing a race condition which can
            // corrupt data if two threads partially update the same piece of data.

            // Because the update event can execute in the thread pool and the method doing the work and reporting progress can be asynchronous,
            // the asynchronous method may complete before the thread pool has time to execute all of the updates. This is probably worst, where
            // executing updates in a fast loop and the queue feeding the thread pool gets backed up. If the progress value is updated after
            // the method doing the work completes i.e. setting to complete, it may cause race conditions with to be executed event handlers.

            // The result of these effects is that the object reported by Progress.Report should be immutable. A new object should be created
            // for each update, preventing race conditions.

            // Each progress update Posted to the synchronization context/thread pool is executed asynchronously 
            var progress = new Progress<double>();
            progress.ProgressChanged += (sender, args) =>
            {
                Console.WriteLine($"Complete: {100 * args}%");
            };
            await MyMethodAsync(progress);
        }

    }
}

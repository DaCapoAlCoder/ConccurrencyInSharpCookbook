using Common;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter02
{
    public class Chapter02_01 : IChapterAsync
    {
        public async Task Run()
        {
            var value = await DownloadStringWithTimeout();

            //Read key pauses the execution of the main thread. 
            //If the cancellation task completes first, the mock download task still executes fully
            //The final trace from the DoSomethingAsync method still outputs while the main thread waits for a key input
            //This means that the soft cancellation using WhenAny does not cause the other task to cancel
            //It still runs to completion

            // Its better to pass a cancellation token directly to the task doing work. The approach here is fine
            // however, if the task doing the work cannot not be cancelled
            Console.ReadKey();
            Console.WriteLine(value is null ? "null" : value);
        }

        private async Task<string> DownloadStringWithTimeout()
        {
            // Create a cancellation token source that will timeout in 3 seconds
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

            // Get the task without actually starting it
            Task<string> mockDownloadTask = DoSomethingAsync();

            // Create a task that times out based on the token source's token
            // The task hasn't started yet
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);

            // Run both tasks, WhenAny returns whichever task completes first
            Task completedTask = await Task.WhenAny(mockDownloadTask, timeoutTask);
            Console.WriteLine(mockDownloadTask.IsCompleted);

            //If the completed task is the timeout task return null otherwise return a result
            //from the mock download task
            if (completedTask == timeoutTask)
            {
                return null;
            }

            return await mockDownloadTask;
        }

        private async Task<string> DoSomethingAsync()
        {
            int value = 13;
            Trace.WriteLine(value);

            // Asynchronously wait 3 seconds.
            await Task.Delay(TimeSpan.FromSeconds(3));

            value *= 2;

            // Asynchronously wait 3 second.
            await Task.Delay(TimeSpan.FromSeconds(3));


            // Write out the final result.
            // This line will still output even if the cancellation task completes first
            Trace.WriteLine(value);
            return value.ToString();
        }
    }
}

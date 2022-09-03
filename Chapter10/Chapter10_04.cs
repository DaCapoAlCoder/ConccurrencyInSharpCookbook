using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter10
{
    public class Chapter10_04 : IChapterAsync
    {
        public async Task Run()
        {
            // Most async APIs already support cancellation so the
            // the approach to supporting cancellation is to simply take
            // a token and pass it to the next layer. The rule of thumb is
            // if you call a method that takes a cancellation token, then
            // your method should also take a cancellation token
            using CancellationTokenSource cts = new();
            var task = CancelableMethodAsync(cts.Token);
            await Task.Delay(TimeSpan.FromSeconds(1));
            cts.Cancel();
            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Async method cancelled");
            }

            // If the called method doesn't support cancellation there is
            // no simple solution, its not possible to safely stop an arbitrary 
            // piece of code from executing without wrapping it in a separate executable

        }

        public async Task<int> CancelableMethodAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("method executing");
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            return 42;
        }
    }
}

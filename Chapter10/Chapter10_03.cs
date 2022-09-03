using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter10
{
    public class Chapter10_03 : IChapterAsync
    {
        // Cancellation naturally lends itself to timeout functionality.
        // A token can be configured to timeout a process after a given timespan
        public async Task Run()
        {
            try
            {
                await IssueTimeoutAsyncFromConstructor();
            }
            catch (OperationCanceledException)
            { 
                Console.WriteLine("Operation timed out using the constructor approach");
            }

            try
            {
                using var cts = new CancellationTokenSource();
                await IssueTimeoutAsyncFromInstance(cts);
            }
            catch (OperationCanceledException)
            { 
                Console.WriteLine("Operation timed out using the method approach");
            }
        }
        
        async Task IssueTimeoutAsyncFromConstructor()
        {
            // A timeout can be specified in the constructor
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            CancellationToken token = cts.Token;
            await Task.Delay(TimeSpan.FromSeconds(10), token);
        }

        async Task IssueTimeoutAsyncFromInstance(CancellationTokenSource cts)
        {
            // If the token source is already created a method can be used 
            // to add a timeout to the token
            CancellationToken token = cts.Token;
            cts.CancelAfter(TimeSpan.FromSeconds(5));
            await Task.Delay(TimeSpan.FromSeconds(10), token);
        }
    }
} 
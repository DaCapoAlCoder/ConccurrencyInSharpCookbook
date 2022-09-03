using Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter10
{
    public class Chapter10_01 : IChapterAsync
    {
        public async Task Run()
        {
            try
            {
                IssueCancelRequest();
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was cancelled");
            }

            // When cancelling an operation, a race condition exists. The operation
            // must check for cancellation periodically, leaving three possible outcomes

            // The operation may complete after cancellation has been requested but before the operation
            // checks for cancellation again
            await IssueCancelRequestAsync(Outcome.Success);
            Console.WriteLine("The operation completed successfully");

            // The cancellation may win the race condition and the operation cancels
            // before it completes
            try
            {
                await IssueCancelRequestAsync(Outcome.Cancel);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("The operation was cancelled");
            }

            // The operation may complete with an unrelated error
            try
            {
                await IssueCancelRequestAsync(Outcome.Error);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("The operation experienced a non-cancellation error");
            }

            // Once cancellation can only be issued once. To re-issue cancellation a new token
            // is required. In other words a cancellation token cannot be un-cancelled.


        }
        public async Task<int> CancelableMethodAsync(Outcome outcome, CancellationToken cancellationToken)
        {
            if(outcome == Outcome.Success)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), CancellationToken.None);
                return 42;
            }

            if(outcome == Outcome.Cancel)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                return 42;
            }
            
            if(outcome == Outcome.Error)
            {
                var task = Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                throw new InvalidOperationException();
            }

            return 42;

        }

        public async Task<int> CancelableMethodAsync(CancellationToken cancellationToken)
        {
            return await CancelableMethodAsync(Outcome.Cancel, cancellationToken);
        }

        void IssueCancelRequest()
        {
            // This method is essentially async void. Its not good code.
            // It was just done this way for the convenience of the author,
            // for some reason

            using var cts = new CancellationTokenSource();
            var task = CancelableMethodAsync(cts.Token);

            // At this point, the operation has been started.

            // Issue the cancellation request.
            cts.Cancel();

            // Blocking the thread here was added to the example code to
            // allow the cancellation to be propagated from the example code
            task.GetAwaiter().GetResult();
        }

        async Task IssueCancelRequestAsync(Outcome outcome)
        {
            using var cts = new CancellationTokenSource();
            var task = CancelableMethodAsync(outcome, cts.Token);

            // At this point, the operation is happily running.

            // Issue the cancellation request.
            cts.Cancel();

            // (Asynchronously) wait for the operation to finish.
            try
            {
                int value = await task;
                // If we get here, the operation completed successfully
                //  before the cancellation took effect.
                Console.WriteLine($"The task completed with the value {value}");
            }
            catch (OperationCanceledException)
            {
                // If we get here, the operation was cancelled before it completed.
                Console.WriteLine("Operation was cancelled");
            }
            catch (Exception)
            {
                // If we get here, the operation completed with an error
                //  before the cancellation took effect.
                Console.WriteLine("Operation failed with another exception");

                //Can re-throw to the caller if required
                //throw;
            }
        }

        public enum Outcome
        {
            Cancel,
            Error,
            Success
        }
    }
}

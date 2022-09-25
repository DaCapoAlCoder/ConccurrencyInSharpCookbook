using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter11
{
    public class Chapter11_06 : IChapterAsync
    {
        public async Task Run()
        {
            // This set-up uses the dispose method to cancel the resource of an async method
            Task<int> task;
            using (CancelByDispose cancelByDispose = new())
            {
                task = cancelByDispose.CalculateValueAsync();
            }

            try
            {
                await task;
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Task cancelled by disposal");
            }

            using (var resource = new CancelByDisposePlusUserToken())
            {
                task = resource.CalculateValueAsync(default);
            }

            // Throws OperationCanceledException.
            try
            {
                var result = await task;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Task cancelled by disposal");
            }

            await TestAsyncDisposal();
            await TestAsyncDispoalWithoutContext();
        }
        class CancelByDispose : IDisposable
        {
            private readonly CancellationTokenSource _disposeCts =
                new CancellationTokenSource();

            public async Task<int> CalculateValueAsync()
            {
                await Task.Delay(TimeSpan.FromSeconds(2), _disposeCts.Token);
                return 13;
            }

            public void Dispose()
            {
                // This is just a basic implementation, further code is required for production
                // such as checking if the token is already disposed or a means to supply a 
                // predefined token
                _disposeCts.Cancel();
            }
        }

        class CancelByDisposePlusUserToken : IDisposable
        {
            private readonly CancellationTokenSource _disposeCts =
                new CancellationTokenSource();

            public async Task<int> CalculateValueAsync(CancellationToken cancellationToken)
            {
                // This implementation allows the consumer to add their own cancellation token
                // so that they can cancel independently of the internal token source. The 
                // combined token may seem redundant but if disposed the user token will not
                // see cancellation requested
                using CancellationTokenSource combinedCts = CancellationTokenSource
                    .CreateLinkedTokenSource(cancellationToken, _disposeCts.Token);
                await Task.Delay(TimeSpan.FromSeconds(2), combinedCts.Token);
                return 13;
            }

            public void Dispose()
            {
                _disposeCts.Cancel();
            }
        }

        class AsyncDispose : IAsyncDisposable
        {
            public async ValueTask DisposeAsync()
            {
                Console.WriteLine("Start disposing of asynchronous resources");
                await Task.Delay(TimeSpan.FromSeconds(2));
                Console.WriteLine("Finish disposing of asynchronous resources");
            }
        }

        async Task TestAsyncDisposal()
        {
            await using (var myClass = new AsyncDispose())
            {
                // ...
            } // DisposeAsync is invoked (and awaited) here
        }

        async Task TestAsyncDispoalWithoutContext()
        {
            var myClass = new AsyncDispose();
            await using (myClass.ConfigureAwait(false))
            {
                // ...
            } // DisposeAsync is invoked (and awaited) here with ConfigureAwait(false)
        }
    }
}

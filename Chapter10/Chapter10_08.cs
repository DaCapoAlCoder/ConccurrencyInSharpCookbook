using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter10
{
    public class Chapter10_08 : IChapterAsync
    {
        public async Task Run()
        {
            CancellationTokenSource cts = new();
            var task = GetWithTimeoutAsync(new HttpClient(), "https://example.com", cts.Token, false);
            cts.Cancel();
            await task;

            cts.Dispose();
            cts = new();
            await GetWithTimeoutAsync(new HttpClient(), "https://example.com", cts.Token, true);
            cts.Dispose();
        }

        async Task<HttpResponseMessage> GetWithTimeoutAsync(HttpClient client,
            string url, CancellationToken cancellationToken, bool cancelInternal)
        {
            HttpResponseMessage httpResponseMessage = default;

            // The combined token will cancel when called explicitly, this does not affect
            // the linked token. When the linked token is cancelled the combined token will cancel
            // In other words cancelling outside cancels both, cancelling inside only cancels inside
            using CancellationTokenSource cts = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken);

            cts.CancelAfter(TimeSpan.FromSeconds(1));
            CancellationToken combinedToken = cts.Token;

            if (cancelInternal)
            {
                // Wait for a second or so for the internal cancellation token to cancel
                await Task.Delay(TimeSpan.FromSeconds(1.2));
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1.5));
                httpResponseMessage = await client.GetAsync(url, combinedToken);

            }
            catch (Exception)
            {
                Console.WriteLine($"External token is cancelled: {cancellationToken.IsCancellationRequested}");
                Console.WriteLine($"Internal token is cancelled: {cts.IsCancellationRequested}");
            }
            return httpResponseMessage;
        }
    }
}

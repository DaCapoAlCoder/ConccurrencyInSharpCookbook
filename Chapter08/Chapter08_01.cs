using Common;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Chapter08
{
    public class Chapter08_01 : IChapterAsync
    {
        public async Task Run()
        {
            // This client is an older one, the HttpClient should
            // actually be used instead

            // This WebClient uses an older Event Async Pattern (EAP)
            // that follows an OperationAsync and OperationCompleted format
            WebClient webClient = new();
            Uri uri = new("https://example.com/");
            // The WebClient already defines a method that fulfils
            // the same purpose, this is just a demo. Note that
            // the original method is already Async so the wrapper
            // uses the suffix TaskAsync
            var value = await webClient.CustomDownloadStringTaskAsync(uri);
            Console.WriteLine(value);
        }
    }

    public static class WebClientExtensions
    {
        public static Task<string> CustomDownloadStringTaskAsync(this WebClient client, Uri address)
        {
            var tcs = new TaskCompletionSource<string>();

            // The event handler will complete the task and unregister itself.
            DownloadStringCompletedEventHandler handler = null;
            handler = (_, e) =>
            {
                // unregister the handler as soon as the download event fires
                // In production this would need error handling to do something
                // similar
                client.DownloadStringCompleted -= handler;

                // Set the state of the task managed by the TaskCompletionSource
                if (e.Cancelled)
                    tcs.TrySetCanceled();
                else if (e.Error != null)
                    tcs.TrySetException(e.Error);
                else
                    tcs.TrySetResult(e.Result);
            };

            // Register for the event and *then* start the operation.
            client.DownloadStringCompleted += handler;

            // Note that this isn't being awaited, this is an async method implemented
            // in an older format that returns void. The point here is to wrap the older
            // implementation in something that can use async await
            client.DownloadStringAsync(address);

            return tcs.Task;
        }
    }
}

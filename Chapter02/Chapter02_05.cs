using Common;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Chapter02
{
    public class Chapter02_05 : IChapterAsync
    {
        public async Task Run()
        {
            var byteCount = await FirstRespondingUrlAsync(new HttpClient(), "https://www.google.com", "https://www.bing.com");
        }

        async Task<int> FirstRespondingUrlAsync(HttpClient client, string urlA, string urlB)
        {
            // Start both downloads concurrently.
            Task<byte[]> downloadTaskA = client.GetByteArrayAsync(urlA);
            Task<byte[]> downloadTaskB = client.GetByteArrayAsync(urlB);

            // Wait for either of the tasks to complete.
            // WhenAny returns an inner Task when awaited
            // This outter task returned from the initial await wont complete as faulted or cancelled.
            // It always completes sucessfully

            // WhenAny can be combined with a delay task to create a timeout. However the non-timeout task
            // will still run after the delay expires. This in turn will require it be cancelled. It is
            // better to just use a cancellation token with a timeout built in (as overload on the constructor)
            // as this does both jobs of timeing out and cancelling.

            // Its possible to use WhenAny to maintain a list of tasks and process them as they complete
            // however, this results in an O(N^2) operation and cannot be used for large amount of tasks
            // Section 2.6 shows a better implementation of handling tasks as they complete
            Task<byte[]> completedTask =
                await Task.WhenAny(downloadTaskA, downloadTaskB);

            // Once we know which task has been completed, the other tasks should be cancelled,
            // as they will continue to run. Non complete task's exceptions will be ignored
            if(completedTask == downloadTaskA)
            {
                Console.WriteLine($"Task A won. String downloaded from: {urlA}");
            }
            else
            {
                Console.WriteLine($"Task B won. String downloaded from: {urlB}");
            }

            // Return the length of the data retrieved from that URL.
            // To get the data the inner task must be awaited
            // Since the outter task always completes successfully this inner task
            // will throw any exceptions that have occurred when awaited
            byte[] data = await completedTask;
            int dataLength = data.Length;
            Console.WriteLine($"The number of bytes returned is: {dataLength}");
            return dataLength;

        }
    }
}

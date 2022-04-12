using Chapter3.CommonApi;
using Common;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Chapter3
{
    public class Chapter3_02 : IChapter
    {
        public async Task Run()
        {
            await ProcessValueAsync(new HttpClient());
            await DoAdditionalAsyncWorkAsync(new HttpClient());
        }

        IAsyncEnumerable<string> GetValuesAsync(HttpClient client) => new ApiValues().GetValuesAsync(client);

        public async Task ProcessValueAsync(HttpClient client)
        {
            // Consuming an asynchronous stream is done by awaiting a foreach loop
            // GetValuesAsync returns an asynchronous enumerable and the foreach creates
            // an asynchronous enumerator. The await in foreach will await the next value
            // to come from the asynchronous enumerable then execute the body.
            await foreach (string value in GetValuesAsync(client))
            {
                Console.WriteLine(value);
            }
        }

        public async Task DoAdditionalAsyncWorkAsync(HttpClient client)
        {
            await foreach (string value in GetValuesAsync(client))
            {
                // Its possible to do further asynchronous processing 
                // inside of the loop body. Demonstrates both the functionality
                // to get the data for the loop is asynchronous as well as the loop body
                await Task.Delay(100); // asynchronous work
                Console.WriteLine(value);
            }
        }

        public async Task DoAdditionalWorkDoReturnToContextAsync(HttpClient client)
        {
            await foreach (string value in GetValuesAsync(client).ConfigureAwait(false))
            {
                // There is a hidden await in the operation that gets the next element
                // of the enumerator for the foreach loop. Using configure await here
                // will pass that configuration to the hidden awaits. An await is also
                // generated to asynchronously dispose the enumerable
                await Task.Delay(100).ConfigureAwait(false); // asynchronous work
                Console.WriteLine(value);
            }
        }
    }
}

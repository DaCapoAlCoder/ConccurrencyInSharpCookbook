using Chapter3.CommonApi;
using Common;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Chapter3
{
    public class Chapter3_01 : IChapter
    {
        // API to use for example
        public async Task Run()
        {
            ApiValues apiValues = new();
            await foreach (var val in GetValuesAsync())
            {
                Console.WriteLine($"Value {val}");
            }

            // This will query the API for ten Pokemon at a time then
            // print each one before launching the next query to get
            // the next ten and so on. This asynchronous stream allows
            // the processing to happen without having to fetch and
            // populate the full list of pokemon first
            Console.WriteLine($"Printing Pokemon names in blocks of 10");
            await foreach (var val in apiValues.GetValuesAsync(new HttpClient()))
            {
                Console.WriteLine($"Value {val}");
            }
        }

        async IAsyncEnumerable<int> GetValuesAsync()
        {
            await Task.Delay(1000); // some asynchronous work
            yield return 10;
            await Task.Delay(1000); // more asynchronous work
            yield return 13;
        }
    }
}

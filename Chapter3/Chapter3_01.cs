using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Chapter3
{
    public class Chapter3_01 : IChapter
    {
        // API to use for example
        public async Task Run()
        {
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
            await foreach (var val in GetValuesAsync(new HttpClient()))
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

        async IAsyncEnumerable<string> GetValuesAsync(HttpClient client)
        {
            int offset = 0;
            const int limit = 10;
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            while (true)
            {
                // Get the current page of results and parse them
                string result = await client.GetStringAsync($"https://pokeapi.co/api/v2/pokemon?limit={limit}&offset={offset}");
                var pokemonResults = JsonSerializer.Deserialize<PokemonResults>(result, options);
                string[] valuesOnThisPage = pokemonResults.Results.Select(x => x.Name).ToArray();

                // Produce the results for this page
                foreach (string value in valuesOnThisPage)
                    yield return value;

                // If this is the last page, we're done
                if (valuesOnThisPage.Length != limit)
                    break;

                // Otherwise, proceed to the next page
                offset += limit;
            }
        }
        public class PokemonResults
        {
            public int Count { get; set; }
            public string Next { get; set; }
            public string Previous { get; set; }
            public IReadOnlyCollection<Pokemon> Results { get; set; }

        }
        public class Pokemon
        {
            public string Name { get; set; }
            public string Url { get; set; }
        }
    }
}

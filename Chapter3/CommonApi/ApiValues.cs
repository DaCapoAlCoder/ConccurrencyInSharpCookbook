using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

namespace Chapter3.CommonApi
{
    public class ApiValues
    {

        public async IAsyncEnumerable<string> GetValuesAsync(HttpClient client)
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
    }
}
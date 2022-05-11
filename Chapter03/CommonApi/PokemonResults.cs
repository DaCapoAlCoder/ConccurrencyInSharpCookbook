using System.Collections.Generic;

namespace Chapter03.CommonApi
{
    public class PokemonResults
    {
        public int Count { get; set; }
        public string Next { get; set; }
        public string Previous { get; set; }
        public IReadOnlyCollection<Pokemon> Results { get; set; }
    }
}

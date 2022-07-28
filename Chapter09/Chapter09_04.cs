using Common;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter09
{
    public class Chapter09_04 : IChapter
    {
        public void Run()
        {
            // Dictionaries goods for storing reference data in a lookup collection
            // All operations are OLog(N)
            // For all immutable collections builders are available for faster creation
            ImmutableDictionary();
            ImmutableSortedDictionary();
        }
        void ImmutableDictionary()
        {
            // Items in a non-sorted dictionary have an unpredictable order
            ImmutableDictionary<int, string> dictionary = ImmutableDictionary<int, string>.Empty;
            dictionary = dictionary.Add(10, "Ten");
            dictionary = dictionary.Add(21, "Twenty-One");
            // have to use a set item method instead of index based assignment
            // because the new dictionary must be returned
            dictionary = dictionary.SetItem(10, "Murray");

            // Displays "10Murray" and "21Twenty-One" in an unpredictable order.
            foreach (KeyValuePair<int, string> item in dictionary)
                Console.WriteLine(item.Key + item.Value);

            // Can use indexing to access elements
            string ten = dictionary[10];
            // ten == "Murray"

            dictionary = dictionary.Remove(21);
        }
        void ImmutableSortedDictionary()
        {
            // For a sorted dictionary the keys must be directly comparable. A regular dictionary can use any key
            ImmutableSortedDictionary<int, string> sortedDictionary = ImmutableSortedDictionary<int, string>.Empty;
            sortedDictionary = sortedDictionary.Add(10, "Ten");
            sortedDictionary = sortedDictionary.Add(21, "Twenty-One");
            sortedDictionary = sortedDictionary.SetItem(10, "Murray");

            // Displays "10Murray" followed by "21Twenty-One".
            foreach (KeyValuePair<int, string> item in sortedDictionary)
            {
                Console.WriteLine(item.Key + item.Value);
            }

            string ten = sortedDictionary[10];
            // ten == "Murray"

            sortedDictionary = sortedDictionary.Remove(21);
        }
    }
}

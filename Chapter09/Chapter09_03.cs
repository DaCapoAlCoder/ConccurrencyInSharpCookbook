using Common;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter09
{
    public class Chapter09_03 : IChapter
    {
        public void Run()
        {
            // Hash sets can be good for indexing words in a file
            // All operations are OLog(N)
            HashSetUnsorted();
            HashSetSorted();
        }
        void HashSetUnsorted()
        {
            ImmutableHashSet<int> hashSet = ImmutableHashSet<int>.Empty;
            hashSet = hashSet.Add(13);
            hashSet = hashSet.Add(7);

            // Displays "7" and "13" in an unpredictable order.
            Console.WriteLine("Unsorted hash set has no predictable order");
            foreach (int item in hashSet)
            {
                Console.WriteLine(item);
            }

            hashSet = hashSet.Remove(7);
        }

        void HashSetSorted()
        {
            ImmutableSortedSet<int> sortedSet = ImmutableSortedSet<int>.Empty;
            sortedSet = sortedSet.Add(13);
            sortedSet = sortedSet.Add(7);

            // Displays "7" followed by "13".
            Console.WriteLine("Sorted hash enumerates in order");
            //Indexing even with a foreach loop indexing is OLog(N)
            foreach (int item in sortedSet)
            {
                Console.WriteLine(item);
            }
            int smallestItem = sortedSet[0];
            Console.WriteLine($"Smallest element always at index 0 {smallestItem}");

            sortedSet = sortedSet.Remove(7);
        }
    }
}

using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter04
{
    public class Chapter04_05 : IChapter
    {
        public void Run()
        {
            // Almost anything that can be done with LINQ can be done in PLINQ
            // There are parallel equivalents for most LINQ operations
            // Parallel is good for some use cases but PLINQ code is simpler
            // PLINQ will use all resources available which may cause issues in the server
            // Parallel is more discerning and reacts to current CPU conditions
            int arraySize = 100_000_000;
            int[] array = new int[arraySize];
            Random rand = new();
            for(int i = 0; i < arraySize; i++)
            {
                array[i] = rand.Next(1, 10);
            }

            var multiplied = MultiplyBy2(array);
            var multiplied2 = MultiplyBy2Ordered(array);

            var total = ParallelSum(multiplied);
            Console.WriteLine($"Sum = {total}");

        }

        IEnumerable<int> MultiplyBy2(IEnumerable<int> values)
        {
            // This will not preserve the order of the elements
            return values.AsParallel().Select(value => value * 2);
        }

        IEnumerable<int> MultiplyBy2Ordered(IEnumerable<int> values)
        {
            // This will does preserve the order of the elements
            return values.AsParallel().AsOrdered().Select(value => value * 2);
        }

        int ParallelSum(IEnumerable<int> values)
        {
            //PLINQ can be used for aggregation
            return values.AsParallel().Sum();
        }
    }
}

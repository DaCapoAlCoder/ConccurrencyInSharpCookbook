using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Chapter04
{
    public class Chapter04_02 : IChapter
    {
        public void Run()
        {
            IList<long> values = GenerateRandomLongs();

            Runner("Parallel.ForEach", ParallelSum, values);
            Runner("PLINQ Sum", ParallelSumPlinq, values);
            Runner("PLINQ Aggregate", ParallelSumAggregate, values);

        }

        private static IList<long> GenerateRandomLongs()
        {
            IList<long> values = new List<long>();
            var rand = new Random();

            Stopwatch sw = new();
            sw.Start();

            for (int i = 0; i < 100_000_000; i++)
            {
                values.Add(rand.Next());
            }

            sw.Stop();
            Console.WriteLine($"Time to generate {values.Count} ints: {sw.Elapsed}");
            return values;
        }

        private void Runner(string type, Func<IList<long>, long> func, IList<long> values)
        {
            Stopwatch sw = new();
            sw.Start();
            var result = func(values);
            sw.Stop();

            Console.WriteLine($"Time to sum {values.Count} ints in parallel using {type}: {sw.Elapsed}");
            Console.WriteLine($"Sum of {values.Count} random ints: {result}");
        }

        // Note: this is not the most efficient implementation.
        // This is just an example of using a lock to protect shared state.
        long ParallelSum(IEnumerable<long> values)
        {
            object mutex = new object();
            long result = 0;
            // This ForEach will sum all of the values
            Parallel.ForEach(source: values,
                // This initialises the thread local variable
                localInit: () => 0L,
                // item is the value from the source, i.e. values parameter passed in to method or the list of longs
                // state that can be used to stop the loop
                // Local value that is local for each thread
                // This is the operation carried out in each thread
                body: (item, state, localValue) => localValue + item,
                // This is the operation to carry out after each thread has completed execution
                // Since there is a local value for each thread, you must protect the shared state which in this case is 
                // the result variable
                localFinally: localValue =>
                {
                    lock (mutex)
                        result += localValue;
                });
            return result;
        }

        long ParallelSumPlinq(IEnumerable<long> values)
        {
            return values.AsParallel().Sum();
        }


        long ParallelSumAggregate(IEnumerable<long> values)
        {
            return values.AsParallel().Aggregate(
                // This is the initial value that will get summed up
                // its unclear but it looks like there would be an instance per thread
                seed: 0L,
                // This is the operation performed on each item in the loop, this will run per thread
                // Unlike ParallelForeach there is no localFinallyMethod, the last summation is carried out internally
                func: (sum, item) => sum + item
            );
        }
    }
}

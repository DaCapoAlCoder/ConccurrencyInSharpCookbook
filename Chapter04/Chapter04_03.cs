using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter04
{
    public class Chapter04_03 : IChapter
    {
        public void Run()
        {
            // Parallel.Invoke is good for invoking mostly independent workloads but if you need to 
            // do something to every element of a collection, then Parallel.Foreach or Parallel.For would
            // be a better option. If each action produces an output PLINQ would be better instead
            var array = CreateArray();
            Stopwatch stopWatch = new();
            stopWatch.Start();
            ProcessPartialArray(array, 0, array.Length);
            stopWatch.Stop();
            Console.WriteLine($"Time to process array with single thread: {stopWatch.Elapsed}");
            stopWatch = new();
            stopWatch.Start();
            ProcessArray(array);
            stopWatch.Stop();
            Console.WriteLine($"Time to process array with two threads: {stopWatch.Elapsed}");

            stopWatch = new();
            stopWatch.Start();
            DoAction5Times(() => ProcessSelfCreatedArray());
            stopWatch.Stop();
            Console.WriteLine($"Time to process in 5 threads: {stopWatch.Elapsed}");

            stopWatch = new();
            stopWatch.Start();
            CancellationTokenSource cts = new(500);
            stopWatch.Stop();
            DoAction5Times(() => ProcessSelfCreatedArray(), cts.Token);
            Console.WriteLine($"Cancelled in: {stopWatch.Elapsed}");
        }

        private double[] CreateArray(int size = 0)
        {
            // Its only at this magnitude before multi-threading became
            // actually faster than single threading
            int iterations = size == 0 ? 50_000_000 : size;
            var array = new double[iterations];
            Random rand = new();
            for (int i = 0; i < iterations; i++)
            {
                array[i] = rand.NextDouble();
            }
            return array;
        }

        void ProcessArray(double[] array)
        {
            // Can invoke individual actions of mostly independent work
            Parallel.Invoke(
                () => ProcessPartialArray(array, 0, array.Length / 2),
                () => ProcessPartialArray(array, array.Length / 2, array.Length)
            );
        }

        void ProcessSelfCreatedArray()
        {
            var array = CreateArray(10_0000);
            ProcessPartialArray(array, 0, array.Length);
        }


        void ProcessPartialArray(double[] array, int begin, int end)
        {
            // Sorting is used as a CPU intensive activity. This won't actually sort the full array
            double[] toSort = new double[end - begin];
            array.Skip(begin).Take(end).ToArray().CopyTo(toSort, 0);
            Array.Sort(toSort);
            Console.WriteLine($"Length of sorted Array {toSort.Length}\n");
        }
        void DoAction5Times(Action action)
        {
            // Parallel.Invoke can take an enumerable of actions/methods
            // Can be useful if the number of threads/invocations is only known at run time
            Action[] actions = Enumerable.Repeat(action, 5).ToArray();
            Parallel.Invoke(actions);
        }


        void DoAction5Times(Action action, CancellationToken token)
        {
            // Parallel can be cancelled
            Action[] actions = Enumerable.Repeat(action, 5).ToArray();
            Parallel.Invoke(new() { CancellationToken = token }, actions);
        }
    } }

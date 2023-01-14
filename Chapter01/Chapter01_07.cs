using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Chapter01
{
    public class Chapter01_07 : IChapter
    {
        public void Run()
        {
            // This example of parallelism uses Parallel linq - PLINQ
            // unlike the Parallel class PLINQ will try and use all of
            // the available resources, so consideration is required
            // as to how best to implement parallel processing for a 
            // given use case
            var values = Enumerable.Range(0, 1000);
            PrimalityTestPlinq(values).ToArray();
        }
        IEnumerable<bool> PrimalityTestPlinq(IEnumerable<int> values)
        {
            return values.AsParallel().Select(value =>
            {
                bool isPrime = IsPrime(value);
                Console.WriteLine($"Checking prime for {value}: {isPrime}, in thread {Thread.CurrentThread.ManagedThreadId}");
                return isPrime;
            });
        }

        bool IsPrime(int n)
        {
            if (n <= 1)
            {
                return false;
            }
            for (int i = 2; i < n; i++)
            {
                if (n % i == 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}

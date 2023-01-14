using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter01
{
    public class Chapter01_06 : IChapter
    {
        public void Run()
        {
            // Parallel programming is used to where there's a lot of computation to
            // do on client systems. Its typically not used on server applications
            // which will have their own parallelism built in such as asp.net core
            // The kind of parallelism used here is data parallelism, the other type
            // is task parallelism

            // The are a couple of ways to do data parallelism one is to use the
            // Parallel class and the other is to parallel linq: PLINQ. This example
            // uses Parallel which is more resource friendly and will work better
            // with other resources on the system. Plinq tries to spread the workload
            // across all CPUs so there is a consideration as to which one to use.

            // The more independent chunks of work are the more it can be parallelised
            // as work becomes interdependent then synchronisation has to be used.
            var matrices = Enumerable.Range(0, 1000).Select(i => new Matrix());

            RotateMatricesParallel(matrices, 10.5f);

        }

        class Matrix
        {
            public void Rotate(float degrees)
            {
                Console.WriteLine($"Rotating by {degrees} degrees, in thread {Thread.CurrentThread.ManagedThreadId}");
            }
        }

        void RotateMatricesParallel(IEnumerable<Matrix> matrices, float degrees)
        {
            // There is a parallel for which can be used if an index is required
            Parallel.ForEach(matrices, matrix => matrix.Rotate(degrees));
        }

    }
}

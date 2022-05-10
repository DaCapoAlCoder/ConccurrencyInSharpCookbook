using Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Threading;
using System.Linq;

namespace Chapter4
{
    [SuppressMessage("Usage", "CA1416", Justification = "Code runs on Windows")]
    public class Chapter4_01 : IChapter
    {
        public void Run()
        {
            // Parallel Linq PLINQ is an alternative to the Parallel library used here.
            // But PLINQ will use all cores, while Parallel reacts dynamically to 
            // current CPU conditions 

            List<Matrix> matrices = CreateMatrices(1000000);

            RotateMatrices(matrices, 30);
            InvertMatrices(matrices);

            var cts = new CancellationTokenSource();
            cts.Cancel();
            RotateMatrices(matrices, 30, cts.Token);

            InvertMatricesWithSharedState(matrices);
        }

        private static List<Matrix> CreateMatrices(int amountToCreate)
        {
            List<Matrix> matrices = new();
            for (int i = 0; i < amountToCreate; i++)
            {
                var rand = new Random();
                int[] numbers = new int[10];
                for (int j = 0; j < 6; j++)
                {
                    numbers[j] = rand.Next(0, 10000);
                }

                var matrix3x2 = new Matrix3x2(numbers[0], numbers[1], numbers[2], numbers[3], numbers[4], numbers[5]);
                var matrix = new Matrix(matrix3x2);
                matrices.Add(matrix);
            }
            //Some random non-invertible matrix
            var nonInvertible = new Matrix3x2(4875, 2912, 9750, 5824, 2375, 6097);
            matrices.Add(new Matrix(nonInvertible));

            return matrices;
        }

        void RotateMatrices(IEnumerable<Matrix> matrices, float degrees)
        {
            Parallel.ForEach(matrices, matrix => matrix.Rotate(degrees));
            Console.WriteLine($"All matrices rotated");
        }

        void InvertMatrices(IEnumerable<Matrix> matrices)
        {
            Parallel.ForEach(matrices, (matrix, state) =>
            {
                if (!matrix.IsInvertible)
                {
                    // state.Stop() is used to stop the loop from within 
                    // there is also a state.Break() which might allow the current iteration to finish
                    // The execution is parallel so there can be other iterations of the loop executing
                    // on separate threads which will run before everything will stop. Items after the 
                    // item in the collection calling stop may also be executing in parallel loops
                    Console.WriteLine("Non-Invertible matrix found");
                    state.Stop();
                }
                else
                {
                    matrix.Invert();
                }
            });
        }
        void RotateMatrices(IEnumerable<Matrix> matrices, float degrees,
            CancellationToken token)
        {
            // Cancellation happens from outside the loop, as opposed to stopping which happens within
            // This sort of cancellation only seems to work reliably when cancelled before the loop
            // starts, its better to rely on state.Stop() if the loop has already started.
            // Even analysing the state of the IsCancellationRequested property on the token doesn't
            // work reliably

            // The book mentions using a Cancel button to cancel the loop. This might work better in a
            // WPF application which will have a synchronisation context. 
            try
            {
                Parallel.ForEach(matrices,
                    new ParallelOptions { CancellationToken = token },
                    (matrix) => matrix.Rotate(degrees));
            }
            catch
            {
                Console.WriteLine("Loops was cancelled");
            }
        }

        // Note: this is not the most efficient implementation.
        // This is just an example of using a lock to protect shared state.
        int InvertMatricesWithSharedState(IEnumerable<Matrix> matrices)
        {
            object mutex = new object();
            int nonInvertibleCount = 0;
            Parallel.ForEach(matrices, matrix =>
            {
                if (matrix.IsInvertible)
                {
                    matrix.Invert();
                }
                else
                {
                    lock (mutex)
                    {
                        ++nonInvertibleCount;
                    }
                }
            });
            Console.WriteLine($"{nonInvertibleCount} matrices couldn't be inverted");
            return nonInvertibleCount;
        }
    }
}

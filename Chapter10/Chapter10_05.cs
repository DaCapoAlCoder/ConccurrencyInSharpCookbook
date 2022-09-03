using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter10
{
    [SuppressMessage("Usage", "CA1416", Justification = "Code runs on Windows")]
    public class Chapter10_05 : IChapterAsync
    {
        public async Task Run()
        {
            Console.WriteLine("Generating matrices");
            List<Matrix> matrices = CreateMatrices(2_000_000);
            Console.WriteLine("Finished generating matrices");

            // Using the options to cancel a parallel operations allows the method to 
            // throw an OperationCanceledException. Using options may also allow the compiler
            // to make a more intelligent decision on how to poll the actual token. This is
            // something that is not possible when passing the token directly into the 
            // executing delegate instead of putting it in an option
            using var cts = new CancellationTokenSource();
            var task = Task.Run(() => RotateMatricesCancelWithOptions(matrices, 30, cts.Token));
            cts.Cancel();
            try
            {
                await task;
                Console.WriteLine("First done");
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine($"Rotation Cancelled with {ex.GetType()}");
            }

            // Its possible to pass the cancellation token into the method directly but
            // this results in an AggregateException being thrown instead of an 
            // OperationCanceledException 
            using var cts2 = new CancellationTokenSource();
            task = Task.Run(() => RotateMatricesCancelWithinMethod(matrices, 30, cts2.Token));
            cts2.Cancel();

            try
            {
                await task;
                Console.WriteLine("Second Done");
            }
            catch (AggregateException ex)
            {
                Console.WriteLine($"Rotation Cancelled with {ex.GetType()}");
                Console.WriteLine($"The inner exception is {ex.InnerException.GetType()}");
            }

           // Parallel Linq has a cancellation method that can be added to the chain
           // of line queries
            using var cts3 = new CancellationTokenSource();
            var numbers = Enumerable.Range(1, int.MaxValue);
            task = Task.Run(() => MultiplyBy2(numbers, cts3.Token).ToList());
            cts3.Cancel();

            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Multiplying by two cancelled");
            }
        }

        // This is the same code used to generate the matrices for examples in Chapter4
        List<Matrix> CreateMatrices(int amountToCreate)
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

        void RotateMatricesCancelWithOptions(IEnumerable<Matrix> matrices, float degrees,
            CancellationToken token)
        {
            // Even though the token is passed as an option, it could also be passed to
            // the executing delegate if it is required in there. The point is that it
            // shouldn't only be passed to the loop body

            Parallel.ForEach(matrices,
                new ParallelOptions { CancellationToken = token },
                matrix => matrix.Rotate(degrees));
        }

        void RotateMatricesCancelWithinMethod(IEnumerable<Matrix> matrices, float degrees,
            CancellationToken token)
        {
            // Warning: not recommended; see below.
            // This will cause an Aggregate exception rather than a Cancelled exception
            Parallel.ForEach(matrices, matrix =>
            {
                matrix.Rotate(degrees);
                token.ThrowIfCancellationRequested();
            });
        }
        IEnumerable<int> MultiplyBy2(IEnumerable<int> values,
            CancellationToken cancellationToken)
        {
            return values.AsParallel()
                .WithCancellation(cancellationToken)
                .Select(item => item* 2);
        }
    }
}

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

namespace Chapter13
{
    [SuppressMessage("Usage", "CA1416", Justification = "Code runs on Windows")]
    public class Chapter13_03 : IChapter
    {
        public void Run()
        {
            // Here, Parallel.ForEach is used with the scheduler but equally
            // Parallel.Invoke can use it in the same way

            // For Tasks, TaskFactory.Start can be used to create parallel tasks
            // that use a given scheduler as well as Task.Continue with (see examples
            // in Chapter13_01 instance.

            //PLINQ has no option to run on a user defined scheduler

            List<List<Matrix>> list = new List<List<Matrix>>();
            for (int i = 0; i < 3; i++)
            {
                list.Add(CreateMatrices(3));
            }

            RotateMatrices(list, 30);
        }
        void RotateMatrices(IEnumerable<IEnumerable<Matrix>> collections, float degrees)
        {
            // Scheduler used to limit concurrency
            var schedulerPair = new ConcurrentExclusiveSchedulerPair(
                TaskScheduler.Default, maxConcurrencyLevel: 3);

            TaskScheduler scheduler = schedulerPair.ConcurrentScheduler;

            // Set up the options to parallel to use the limited scheduler
            ParallelOptions options = new ParallelOptions { TaskScheduler = scheduler };

            // Run Parallel.* type code with the restricted scheduler
            Parallel.ForEach(collections, options,
                matrices => Parallel.ForEach(matrices, options,
                    matrix => {
                        Thread.Sleep(500);
                        Console.WriteLine($"Rotating in thread {Thread.CurrentThread.ManagedThreadId}");
                        matrix.Rotate(degrees); 
                    }));
        }

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

            return matrices;
        }
    }
}

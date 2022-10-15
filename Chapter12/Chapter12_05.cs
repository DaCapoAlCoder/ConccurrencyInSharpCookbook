using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Chapter12
{
    [SuppressMessage("Usage", "CA1416", Justification = "Code runs on Windows")]
    public class Chapter12_05 : IChapterAsync
    {
        public async Task Run()
        {
            // Code is considered too concurrent when parts of the application
            // cannot keep up with others causing items to build up in memory one
            // way to handle this is to throttle the amount of concurrency so only
            // so much can be processed at once. 

            // Throttling may also be necessary if the application is using too much
            // resources such as CPU or network connections. Since end users have
            // low powered computers it is often better to over than under throttle
            // concurrency

            var dataflowBlock = DataflowMultiplyBy2();
            for (int i = 0; i < 20; i++)
            {
                await dataflowBlock.SendAsync(10);
            }

            var list = Enumerable.Range(1, 20);
            ParallelMultiplyBy2(list).ToList();

            var matrices = CreateMatrices(20);
            ParallelRotateMatrices(matrices, 30);

            string[] urls = new string[20];
            Array.Fill(urls, "https://example.com");
            await DownloadUrlsAsync(new HttpClient(), urls);

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

        IPropagatorBlock<int, int> DataflowMultiplyBy2()
        {
            // Dataflow blocks can throttle execution using this options class
            var options = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 5
            };

            return new TransformBlock<int, int>(async data =>
            {
                Console.WriteLine($"Processing in thread: {Thread.CurrentThread.ManagedThreadId}");
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
                return data * 2;
            }, options);
        }

        // Using Parallel LINQ (PLINQ)
        IEnumerable<int> ParallelMultiplyBy2(IEnumerable<int> values)
        {
            return values.AsParallel()
                //PLINQ has a method to set the maximum level of parallelism
                .WithDegreeOfParallelism(5)
                .Select(item =>
               {
                   Console.WriteLine($"Processing in thread: {Thread.CurrentThread.ManagedThreadId}");
                   Thread.Sleep(1000);
                   return item * 2;
               });
        }

        // Using the Parallel class
        void ParallelRotateMatrices(IEnumerable<Matrix> matrices, float degrees)
        {
            // Parallel also has a concurrency throttle options
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 5
            };

            Parallel.ForEach(matrices, options, (matrix) =>
            {
                Console.WriteLine($"Processing in thread: {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(1000);
                matrix.Rotate(degrees);
            });
        }


        async Task<string[]> DownloadUrlsAsync(HttpClient client, IEnumerable<string> urls)
        {
            // This method doesn't need the async synchronisation of the semaphore slim.
            // The construction parameter determines how many threads can enter the WaitAsync()
            // section. In this case it is throttled at 5, for synchronisation it would need to be
            // one
            using var semaphore = new SemaphoreSlim(5);
            Task<string>[] tasks = urls.Select(async url =>
            {
                await semaphore.WaitAsync();
                try
                {
                    Console.WriteLine($"Processing in thread: {Thread.CurrentThread.ManagedThreadId}");
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    return await client.GetStringAsync(url);
                }
                finally
                {
                    semaphore.Release();
                }
            }).ToArray();
            return await Task.WhenAll(tasks);
        }
    }
}

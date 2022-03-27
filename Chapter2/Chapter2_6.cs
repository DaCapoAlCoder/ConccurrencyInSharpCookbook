using Common;
using Nito.AsyncEx;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Chapter2
{
    public class Chapter2_6 : IChapter
    {
        public async Task Run()
        {

            Stopwatch stopwatch = new();
            stopwatch.Start();
            await ProcessTasksInOrderAddedAsync();
            stopwatch.Stop();
            Console.WriteLine($"Processing in order added took {stopwatch.Elapsed}");


            stopwatch = new();
            stopwatch.Start();
            await ProcessTasksInOrderCompletedAsync();
            stopwatch.Stop();
            Console.WriteLine($"Processing by order compmleted took {stopwatch.Elapsed}");

            await UseOrderByCompletionAsync();
        }

        async Task<int> DelayAndReturnAsync(int value)
        {
            await Task.Delay(TimeSpan.FromSeconds(value));
            return value;
        }

        // Currently, this method prints "2", "3", and "1".
        // The desired behavior is for this method to print "1", "2", and "3".
        async Task ProcessTasksInOrderAddedAsync()
        {
            Console.WriteLine("Three tasks added in order with delays: 2, 1 and 3 seconds respecitvely");
            // Create a sequence of tasks.
            Task<int> taskA = DelayAndReturnAsync(2);
            Task<int> taskB = DelayAndReturnAsync(3);
            Task<int> taskC = DelayAndReturnAsync(1);
            // The tasks are already executing asynchronously above
            Task<int>[] tasks = new[] { taskA, taskB, taskC };

            // Await each task in the order they were added to the array
            foreach (Task<int> task in tasks)
            {
                var result = await task;
                Console.WriteLine(result);
            }
            Console.WriteLine("Tasks are processed(output written to console) in the order they were added");
        }

        // This method now prints "1", "2", and "3".
        async Task ProcessTasksInOrderCompletedAsync()
        {
            Console.WriteLine("Three tasks added in order with delays: 2, 1 and 3 seconds respecitvely");
            // Create a sequence of tasks.
            Task<int> taskA = DelayAndReturnAsync(2);
            Task<int> taskB = DelayAndReturnAsync(3);
            Task<int> taskC = DelayAndReturnAsync(1);
            Task<int>[] tasks = new[] { taskA, taskB, taskC };
            // The tasks are already executing asynchronously above

            // This takes the array of tasks and awaits each one in order.
            // Essentially trasnforming the "tasks" array from performing async tasks
            // to an array of Tasks being awaited. The important part here is that the
            // the Select statement is transforming the task to a new method that returns
            // a Task type. The processing (writing to conosle) is included in the new method
            // The line below is an example of turning an array of async methods into a Task array (Task[])
            //      Task[] tarr = new Task[] { DelayAndReturnAsync(2), DelayAndReturnAsync(3), DelayAndReturnAsync(1) };
            // The Select method is doing the same but modifying the methods to await
            Task[] processingTasks = tasks.Select(async t =>
            {
                var result = await t;
                Console.WriteLine(result);
            }).ToArray();

            // WhenAll waits for all processing to complete.
            // Each task must already be awaited for this to work
            await Task.WhenAll(processingTasks);

            // The below piece of code will work, but it still proecesses them
            // in the order added. This is because the processing is done after
            // the WhenAll statement, rather than being included with the methods
            // being awaited

            //var vals = await Task.WhenAll(tasks);
            //foreach(int val in vals)
            //{
            //    Console.WriteLine(val);
            //}


            Console.WriteLine("Tasks are processed(output written to console) in the order they completed");
        }

        async Task UseOrderByCompletionAsync()
        {
            Console.WriteLine("This implementation uses Nitro.Async's OrderByCompletionExtension");
            // This uses an extension method from Stephen Cleary's nuget package: Nito.AsyncEx
            // Create a sequence of tasks.
            Task<int> taskA = DelayAndReturnAsync(2);
            Task<int> taskB = DelayAndReturnAsync(3);
            Task<int> taskC = DelayAndReturnAsync(1);
            Task<int>[] tasks = new[] { taskA, taskB, taskC };

            // Await each one as they complete.
            foreach (Task<int> task in tasks.OrderByCompletion())
            {
                int result = await task;
                Console.WriteLine(result);
            }
        }
    }
}

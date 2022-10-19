using Common;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter13
{
    public class Chapter13_02 : IChapterAsync
    {
        public async Task Run()
        {
            // TaskScheduler.Default is the scheduler most in various .net libraries
            // It schedules threads on to the thread pool. 

            AsyncContext.Run(async () => await Test());
            await Test2();
            await Test3();

            // It possible to use SynchronizationContext.Current to execute code directly
            // on the current synchronisation context. Its better to use await to resume
            // on the context instead, which it will do by default i.e. it will execute 
            // the awaited method in a different thread, then continue on the current
            // once it has awaited the completion of the method.

            // There are platform specific types to execute on a UI thread iOS, WPF and
            // Android all provide a Dispatcher type. UWP has CoreDispatcher and Forms
            // has ISychronizeInvoke. Using these ties the code to the platform.
            // SynchronizationContext is a general purpose abstraction for all of them.

            // The Rx library has a high level abstraction called IScheduler which is 
            // recommend if such an abstraction is required. However older libraries
            // such as Task Parallel Library will only understand the TaskScheduler type

        }

        Task Test()
        {
            // This method runs 3 threads to get random numbers on the thread pool
            // The continue with statement then sums all of those numbers back on the original
            // synchronisation context defined in the AsyncContext.Run method
            Console.WriteLine($"Starting out on {Thread.CurrentThread.ManagedThreadId}");
            // This scheduler captures the current synchronisation context and allows work
            // to be scheduled back on to it. Most UI and Old Asp.Net applications will use
            // a synchronisation context
            TaskScheduler scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var taskFactory = Task.Factory;

            var task1 = Task.Run(GetNumberAsync);
            var task2 = Task.Run(GetNumberAsync);
            var task3 = Task.Run(GetNumberAsync);

            var vals = new int[3];
            return Task.WhenAll(task1, task2, task3).
                ContinueWith(async (vals) =>
                {
                    Console.WriteLine($"Summing numbers on {Thread.CurrentThread.ManagedThreadId}");
                    var arr = await vals;
                    var sum = arr.Sum();
                    return Task.FromResult(sum);
                }, scheduler);
        }


        async Task<int> GetNumberAsync()
        {
            Random rand = new();
            Console.WriteLine($"Getting number from Thread {Thread.CurrentThread.ManagedThreadId}");
            await Task.Delay(TimeSpan.FromSeconds(2));
            return rand.Next();
        }


        async Task Test2()
        {
            var sharedList = new List<string> { "val1", "val2", "val3" };
            // One use for the pair is to use the exclusive scheduler to allow only one thread to execute
            // at a time. The code does execute on the thread pool but it will all execute on the same
            // exclusive scheduler instance
            var schedulerPair = new ConcurrentExclusiveSchedulerPair();
            // One scheduler allows concurrent access the other will only allow one thread at a time
            TaskScheduler concurrent = schedulerPair.ConcurrentScheduler;
            TaskScheduler exclusive = schedulerPair.ExclusiveScheduler;
            TaskFactory concurrentTaskFactory = new(concurrent);

            // When these actions run concurrently they will appear unordered as the threads are
            // allowed to execute simultaneously 
            Action readAction = () =>
            {
                Random rand = new();
                foreach (string val in sharedList)
                {
                    Thread.Sleep(rand.Next(1, 1000));
                    Console.WriteLine($"Reading {val} from list in thread: {Thread.CurrentThread.ManagedThreadId}");
                }
            };

            var task1 = concurrentTaskFactory.StartNew(readAction);
            var task2 = concurrentTaskFactory.StartNew(readAction);
            var task3 = concurrentTaskFactory.StartNew(readAction);
            await Task.WhenAll(task1, task2, task3);

            TaskFactory exclusiveTaskFactory = new(exclusive);
            int valToAdd = 4;
            Action writeAction = () =>
            {
                // In this action the output will start and finish before another starts because the scheduler
                // only allows one thread to execute at a time
                Console.WriteLine($"Start Adding {valToAdd} in thread: {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(50);
                sharedList.Add($"Val{valToAdd}");
                Console.WriteLine($"Finish Adding {valToAdd++} in thread: {Thread.CurrentThread.ManagedThreadId}");
            };

            task1 = exclusiveTaskFactory.StartNew(writeAction);
            task2 = exclusiveTaskFactory.StartNew(writeAction);
            task3 = exclusiveTaskFactory.StartNew(writeAction);
            await Task.WhenAll(task1, task2, task3);
        }

        async Task Test3()
        {
            // This uses the concurrent scheduler to throttle the concurrency level
            // This is different from the throttling used in Chapter 12.5. That throttles
            // async execution and code is not considered executing while it is awaiting
            // This throttles executing code. Semaphore slim throttles at a higher whole method
            // level.

            // This is passes the default scheduler in as ConcurrentExclusiveScheduledPair applies
            // its logic to an existing context. Not specifying the context will likely just use the
            // current default.
            var schedulerPair = new ConcurrentExclusiveSchedulerPair(
                TaskScheduler.Default, maxConcurrencyLevel: 3);
            TaskScheduler scheduler = schedulerPair.ConcurrentScheduler;

            TaskFactory concurrentTaskFactory = new(scheduler);

            var sharedList = new List<string> { "val1", "val2", "val3" };
            Action readAction = () =>
            {
                Random rand = new();
                foreach (string val in sharedList)
                {
                    Thread.Sleep(500);
                    Console.WriteLine($"Reading {val} from list in thread: {Thread.CurrentThread.ManagedThreadId}");
                }
            };

            var tasks = new List<Task>();
            for (int i = 0; i < 9; i++)
            {
                tasks.Add(concurrentTaskFactory.StartNew(readAction));
            }
            
            await Task.WhenAll(tasks);
        }
    }
}

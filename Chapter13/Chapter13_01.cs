using Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter13
{
    public class Chapter13_01 : IChapterAsync
    {
        public async Task Run()
        {
            // Task.Run can be used in GUI applications to push some workload of the UI context
            // and on to a background thread. Usually there is no need to do this in ASP.Net as
            // it already runs on a thread pool thread, pushing it to another often doesn't make
            // sense

            // Task.Run replaces:
            //  - Delegate.BeginInvoke
            //  - ThreadPool.QueueUserWorkItem
            //  - Most uses cases involving Thread (except single-thread apartment threads)

            // There is no need to use Task.Run with Parallel PLINQ or TPL Dataflow Libraries
            // as they will schedule themselves on to the thread pool.

            // For dynamic parallelism Task.Factory.StartNEw instead of Task.Run. Task.Run is
            // set-up to be consumed in async code and does not support parent/child tasks common
            // in dynamic parallel scenarios.
            Console.WriteLine("Start executing a method on a thread pool thread");
            await Test();
            Console.WriteLine("Finish executing a method on a thread pool thread\n");

            Console.WriteLine("Start executing an asynchronous method on a thread pool thread");
            await Test2();
            Console.WriteLine("Finish executing an asynchronous method on a thread pool thread");
        }
        Task Test()
        {

            // Task.Run can handle synchronous code delegates
            Task task = Task.Run(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(2));
            });
            return task;
        }

        Task<int> Test2()
        {
            // Task.Run can handle also handle asynchronous code delegates
            Task<int> task = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                return 13;
            });
            return task;
        }
    }
}

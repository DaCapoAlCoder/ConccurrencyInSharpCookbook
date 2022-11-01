using Common;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Chapter13
{
    public class Chapter13_04 : IChapter
    {
        List<int> _list = new List<int>();
        public void Run()
        {
            // Run from this context rather than doing up a WPF based example
            AsyncContext.Run( async () => { 
                Console.WriteLine($"Starting executing on the \"UI\" thread: {Thread.CurrentThread.ManagedThreadId}");
                await Test(); 
            });

            // With a mesh of blocks A, B and C the ConcurrentExclusiveSchedulerPair.ExclusiveScheduler
            // scheduler can be used with blocks A and C to ensure that they never execute at the same time
            // while B can execute whenever it wants or with as many instances as it needs. It is important
            // to note that this only works for executing threads. As soon as blocks A or C awaits an
            // operation, execution for that thread will stop, and a new thread can start executing within
            // the block that is awaiting. This applies to all synchronisation done using the exclusive
            // scheduler

            // Any Data Flow block can take a scheduler even BufferBlocks which may not execute user code
            // but still has clean up operations that will execute with the given scheduler
        }

        async Task Test()
        {
            // The idea here is to perform some actions in a dataflow mesh on the thread pool
            // then use a scheduler to allow the final block in the mesh to add items back on
            // the original context. This allows the thread pool to carry out work and the result
            // to be added back in to a UI component within the UI synchronisation context

            // Set up options to allow the block to operate in the current synchronisation context
            var options = new ExecutionDataflowBlockOptions
            {
                TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext(),
            };

            // This block doesn't take the options so will execute on the thread pool
            var multiplyBlock = new TransformBlock<int, int>(item => 
            { 
                Console.WriteLine($"Multiplying items on the thread pool thread: {Thread.CurrentThread.ManagedThreadId}");
                return item * 2; 
            });

            // This block takes the options and will execute in the synchronisation context
            var displayBlock = new ActionBlock<int>(
                result =>
                {
                    Console.WriteLine($"Adding items to a component on the \"UI\" thread: {Thread.CurrentThread.ManagedThreadId}");
                    _list.Add(result);
                }, options );
            multiplyBlock.LinkTo(displayBlock);

            multiplyBlock.Post(10);
            multiplyBlock.Complete();
            await multiplyBlock.Completion;
        }
    }
}

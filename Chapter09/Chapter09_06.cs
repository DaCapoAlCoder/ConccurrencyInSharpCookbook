using Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter09
{
    public class Chapter09_06 : IChapterAsync
    {
        private readonly BlockingCollection<int> _blockingQueue =
            new BlockingCollection<int>();

        public async Task Run()
        {
            // One thread adds to the queue and the other removes.
            // Usually one thread doesn't do both
            var task1 = Task.Run(Producer);
            // Can have multiple threads consuming/producing
            var task2 = Task.Run(Consumer);
            var task3 = Task.Run(Consumer2);
            await Task.WhenAll(task1, task2, task3);

            // The BlockingCollection is good for multiple threads
            // but not so good for accessing asynchronously,
            // Channels and BufferBlocks can be used to create non blocking
            // async accessed queues
        }

        void Producer()
        {
            for (int i = 0; i < 10; i++)
            {
                _blockingQueue.Add(7);
                _blockingQueue.Add(13);
            }

            // This will complete the consumer enumerable of the queue
            _blockingQueue.CompleteAdding();
        }

        void Consumer()
        {
            // This will block the thread waiting for something to be added to the queue
            foreach (int item in _blockingQueue.GetConsumingEnumerable())
            {
                Console.WriteLine(item);
            }
        }

        void Consumer2()
        {
            // This will block the thread waiting for something to be added to the queue
            int taken = _blockingQueue.Take();
            Console.WriteLine($"{nameof(Consumer2)} took a single: {taken}");
            
        }
    }
}

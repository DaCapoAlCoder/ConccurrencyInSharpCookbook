using Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter09
{
    public class Chapter09_07 : IChapterAsync
    {
        // In the previous section blocking collection defaults to a queue by using
        // the default constructor. The semantics of operation can be modified by 
        // initialising the collection with classes that implement IProducerConsumerCollection
        // such as stack and bag to get different modes of operation

        // Stacks are ordered first in last out
        BlockingCollection<int> _blockingStack = new BlockingCollection<int>(
            new ConcurrentStack<int>());

        // Bags are unordered
        BlockingCollection<int> _blockingBag = new BlockingCollection<int>(
            new ConcurrentBag<int>());


        public async Task Run()
        {
            var task1 = Task.Run(ProducerStack);
            var task2 = Task.Run(ConsumerStack);
            await Task.WhenAll(task1, task2);

            task1 = Task.Run(ProducerBag);
            task2 = Task.Run(ConsumerBag);
            await Task.WhenAll(task1, task2);
        }
        void ProducerStack()
        {
            // Producer code
            for (int i = 0; i < 10; i++)
            {
                _blockingStack.Add(i);
            }
                _blockingStack.CompleteAdding();

        }

        void ProducerBag()
        {
            // Producer code
            for (int i = 0; i < 10; i++)
            {
                _blockingBag.Add(i);
            }
                _blockingBag.CompleteAdding();

        }
        void ConsumerStack()
        {
            Console.WriteLine($"Can use take with stacks:{_blockingStack.Take()}");
            foreach (int item in _blockingStack.GetConsumingEnumerable())
            {
                Console.WriteLine(item);
            }
        }

        void ConsumerBag()
        {
            Console.WriteLine($"Can use take with bags:{_blockingBag.Take()}");
            foreach (int item in _blockingBag.GetConsumingEnumerable())
            {
                Console.WriteLine(item);
            }
        }
    }
}

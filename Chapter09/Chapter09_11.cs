using Common;
using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter09
{
    public class Chapter09_11 : IChapterAsync
    {
        public async Task Run()
        {
            // AsyncCollection is the same as BlockingCollection<T> but with a different
            // interface. Its better to just use BlockingCollection as it doesn't rely on
            // third party library

            // This async collection from the Nito library acts like a queue by default 
            // but will wrap other concurrent collections to adopt their behaviour
            AsyncCollection<int> asyncStack = new(new ConcurrentStack<int>());
            AsyncCollection<int> asyncBag = new(new ConcurrentBag<int>());

            // When the producer and consumer of the stack run concurrently, the 
            // behaviour won't work exactly like a stack, since a consumer can take
            // an element while the producer is still adding one in a separate thread
            // If the producer completes before consumers start then it will behave like a stack
            var task1 = AsyncCollectionProducer(asyncStack, typeof(ConcurrentStack<int>));
            var task2 =  CollectionConsumer(asyncStack);
            await Task.WhenAll(task1, task2);

            // The bag has no ordering at all if the order doesn't matter then this is a
            // better choice
            task1 = AsyncCollectionProducer(asyncBag, typeof(ConcurrentBag<int>));
            task2 =  CollectionConsumer(asyncBag);
            await Task.WhenAll(task1, task2);

            asyncStack = new AsyncCollection<int>(
                new ConcurrentStack<int>(), maxCount: 1);

            task1 = AsyncCollectionProducerThrottled(asyncStack, typeof(ConcurrentStack<int>));
            task2 =  CollectionConsumerThrottled(asyncStack);
            await Task.WhenAll(task1, task2);



        }

        async Task AsyncCollectionProducer(AsyncCollection<int> collection, Type type)
        {
            // Producer code
            await collection.AddAsync(7);
            Console.WriteLine($"Added 7 to Async-{type.Name}".Trim('`', '1'));
            await collection.AddAsync(13);
            Console.WriteLine($"Added 13 to Async-{type.Name}".Trim('`', '1'));
            collection.CompleteAdding();
            Console.WriteLine($"Added complete to Async-{type.Name}".Trim('`', '1'));
        }

        async Task CollectionConsumer(AsyncCollection<int> collection)
        {

            // Consumer code
            // Displays "13" followed by "7".
            while (await collection.OutputAvailableAsync())
            {
                Console.WriteLine($"Took: {await collection.TakeAsync()}");
            }
        }

        async Task AsyncCollectionProducerThrottled(AsyncCollection<int> asyncStack, Type type)
        {
            // This Add completes immediately.
            await asyncStack.AddAsync(7);
            Console.WriteLine($"Added 7 to throttled Async-{type.Name}".Trim('`', '1'));
            Console.WriteLine($"Waiting for 7 to be removed before adding the next  value");

            // This Add (asynchronously) waits for the 7 to be removed
            // before it enqueues the 13.
            await asyncStack.AddAsync(13);
            Console.WriteLine($"Added 13 to throttled Async-{type.Name}".Trim('`', '1'));
            asyncStack.CompleteAdding();
            Console.WriteLine($"Added complete to Async-{type.Name}".Trim('`', '1'));
        }


        async Task CollectionConsumerThrottled(AsyncCollection<int> asyncStack)
        {
            await Task.Delay(2000);

            while (true)
            {
                int item;
                try
                {
                    item = await asyncStack.TakeAsync();
                    Console.WriteLine($"Took {item}");
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine($"No items to take, breaking the loop");
                    break;
                }
            }
        }
    }
}

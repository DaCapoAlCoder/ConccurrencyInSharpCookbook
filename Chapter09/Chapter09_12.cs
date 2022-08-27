using Common;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Chapter09
{
    public class Chapter09_12 : IChapterAsync
    {
        public async Task Run()
        {
            // This is all the same code as was used in chapter 9.8
            var queue = new BufferBlock<int>();
            var task1 = BufferBlockProducerAsync(queue);
            var task2 = BufferBlockConsumerAsync(queue);
            await Task.WhenAll(task1, task2);

            queue = new BufferBlock<int>();
            task1 = BufferBlockProducerAsync(queue);
            Console.WriteLine("Started async producer task thread is not blocked");
            task2 = BufferBlockMultiConsumerAsync(queue, 1);
            Console.WriteLine("Started async consumer task 1 thread is not blocked");
            var task3 = BufferBlockMultiConsumerAsync(queue, 2);
            Console.WriteLine("Started async consumer task 2 thread is not blocked");
            await Task.WhenAll(task1, task2, task3);

            // Synchronous APIs exist for buffer block too
            queue = new BufferBlock<int>();
            Console.WriteLine("Starting Synchronous Producer, current thread blocked");
            SynchronousProducer(queue);
            Console.WriteLine("Finished Synchronous Producer, thread unblocked");

            Console.WriteLine("Starting Synchronous Consumer, current thread blocked");
            SynchronousMultiConsumer(queue, 1);
            Console.WriteLine("Finished Synchronous Consumer, thread unblocked");

            // The point of this section is to demonstrate asynchronous producers/consumers
            // can be combined with synchronous producers/consumers. For example push data
            // synchronously in one thread, while asynchronously consuming it in a UI thread
            // allowing the UI to remain responsive

            queue = new BufferBlock<int>();
            task1 = Task.Run(() =>
            {
                Console.WriteLine("Starting Synchronous Producer, current thread blocked");
                SynchronousProducer(queue);
                Console.WriteLine("Finished Synchronous Producer, thread unblocked");
            });
            task2 = BufferBlockConsumerAsync(queue);
            Console.WriteLine("Started async consumer task thread is not blocked");

            await Task.WhenAll(task1, task2);

            // Using buffer blocks as above is not really within the data flow paradigm. Its possible to link them to action
            // blocks instead with the consumer code being added as an Action<T> delegate. This can be both synchronous and 
            // asynchronous
            ActionBlock<int> actionBlockQueue = new ActionBlock<int>(item => Console.WriteLine($"Action Block Async: {item}"));
            await ActionBlockAsyncProducer(actionBlockQueue);
            actionBlockQueue = new ActionBlock<int>(item => Console.WriteLine($"Action Block Sync: {item}"));
            ActionBlockSynchronousProducer(actionBlockQueue);


            // The Nito library has a similar API to Buffer Blocks with synchronous and asynchronous implementations
            // Using Data Flow blocks is recommended if possible though
            var nitoQueue = new AsyncProducerConsumerQueue<int>();
            task1 = NitoProducerAsync(nitoQueue);
            task2 = NitoSingleConsumerAsync(nitoQueue);
            await Task.WhenAll(task1, task2);

            nitoQueue = new AsyncProducerConsumerQueue<int>();
            task1 = NitoProducerAsync(nitoQueue);
            await NitoMultiConsumerAsync(nitoQueue, 1);
            await NitoMultiConsumerAsync(nitoQueue, 2);

            nitoQueue = new AsyncProducerConsumerQueue<int>();
            NitoPoducerSynchronous(nitoQueue);
            NitoSingleConsumerSynchronous(nitoQueue);

            // Channels are naturally asynchronous, but they can be made to be
            // synchronous by wrapping it in Task.Run and the use GetAwaiter GetResult
            // to block the current thread.
            Channel<int> channelQueue = Channel.CreateBounded<int>(10);

            ChannelForcedSynchronousProducer(channelQueue);

            ChannelForcedSynchronousConsumer(channelQueue);
        }

        async Task BufferBlockProducerAsync(BufferBlock<int> queue)
        {
            // Producer code
            await queue.SendAsync(7);
            Console.WriteLine("Pushed 7 to queue");
            await queue.SendAsync(13);
            Console.WriteLine("Pushed 13 to queue");
            queue.Complete();
        }

        async Task BufferBlockConsumerAsync(BufferBlock<int> queue)
        {

            // Consumer code for a single consumer
            while (await queue.OutputAvailableAsync())
            {
                Console.WriteLine($"Received {await queue.ReceiveAsync()} asynchronously");
            }

        }

        async Task BufferBlockMultiConsumerAsync(BufferBlock<int> queue, int id)
        {
            // The delay helps one thread not dominate in the race condition
            await Task.Delay(10);

            // Consumer code for multiple consumers
            while (true)
            {
                int item;
                try
                {
                    item = await queue.ReceiveAsync();
                    Console.WriteLine($"Received {item} from consumer {id} asynchronously");

                }
                catch (InvalidOperationException)
                {
                    break;
                }

                Console.WriteLine(item);
            }
        }

        void SynchronousProducer(BufferBlock<int> queue)
        {
            // Producer code
            queue.Post(7);
            Console.WriteLine("Pushed 7 to queue");
            queue.Post(13);
            Console.WriteLine("Pushed 13 to queue");
            queue.Complete();
        }

        void SynchronousMultiConsumer(BufferBlock<int> queue, int id)
        {
            // Consumer code
            while (true)
            {
                int item;
                try
                {
                    item = queue.Receive();
                    Console.WriteLine($"Received {item} from consumer {id}");
                }
                catch (InvalidOperationException)
                {
                    break;
                }

            }
        }

        async Task ActionBlockAsyncProducer(ActionBlock<int> queue)
        {
            await queue.SendAsync(7);
            Console.WriteLine("Pushed 7 to queue");
            await queue.SendAsync(13);
            Console.WriteLine("Pushed 13 to queue");
            queue.Complete();
            await queue.Completion;
        }

        void ActionBlockSynchronousProducer(ActionBlock<int> queue)
        {
            queue.Post(7);
            Console.WriteLine("Pushed 7 to queue");
            queue.Post(13);
            Console.WriteLine("Pushed 13 to queue");
            queue.Complete();
        }

        void NitoSingleConsumerSynchronous(AsyncProducerConsumerQueue<int> queue)
        {
            // Synchronous consumer code
            foreach (int item in queue.GetConsumingEnumerable())
            {
                Console.WriteLine($"Nito single consumer received {item}");
            }
        }

        async Task NitoMultiConsumerAsync(AsyncProducerConsumerQueue<int> queue, int id)
        {
            // The delay helps one thread not dominate in the race condition
            await Task.Delay(10);

            // Asynchronous multi-consumer code
            while (true)
            {
                int item;
                try
                {
                    item = await queue.DequeueAsync();
                    Console.WriteLine($"Nito Multi-Consumer {id} Received {item} asynchronously");
                }
                catch (InvalidOperationException)
                {
                    break;
                }
            }
        }

        async Task NitoSingleConsumerAsync(AsyncProducerConsumerQueue<int> queue)
        {
            // Asynchronous single consumer code
            while (await queue.OutputAvailableAsync())
            {
                Console.WriteLine($"Received {await queue.DequeueAsync()} asynchronously");
            }
        }

        void NitoPoducerSynchronous(AsyncProducerConsumerQueue<int> queue)
        {
            // Synchronous producer code
            queue.Enqueue(7);
            Console.WriteLine("Pushed 7 to queue");
            queue.Enqueue(13);
            Console.WriteLine("Pushed 13 to queue");

            queue.CompleteAdding();
        }

        async Task NitoProducerAsync(AsyncProducerConsumerQueue<int> queue)
        {
            // Asynchronous producer code
            await queue.EnqueueAsync(7);
            Console.WriteLine("Pushed 7 to queue");
            await queue.EnqueueAsync(13);
            Console.WriteLine("Pushed 13 to queue");

            queue.CompleteAdding();
        }

        void ChannelForcedSynchronousConsumer(Channel<int> queue)
        {
            // Consumer code
            ChannelReader<int> reader = queue.Reader;
            Task.Run(async () =>
            {
                while (await reader.WaitToReadAsync())
                {
                    while (reader.TryRead(out int value))
                    {
                        Console.WriteLine($"Channel forced synchronous read: {value}");
                    }
                }
            }).GetAwaiter().GetResult();
        }

        void ChannelForcedSynchronousProducer(Channel<int> queue)
        {
            // Producer code
            ChannelWriter<int> writer = queue.Writer;
            Task.Run(async () =>
            {
                await writer.WriteAsync(7);
                        Console.WriteLine($"Channel forced synchronous wrote: 7");
                await writer.WriteAsync(13);
                        Console.WriteLine($"Channel forced synchronous wrote: 13");
                writer.Complete();
            }).GetAwaiter().GetResult();
        }
    }
}

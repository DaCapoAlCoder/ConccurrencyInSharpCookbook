using Common;
using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Chapter09
{
    public class Chapter09_09 : IChapterAsync
    {
        public async Task Run()
        {
            // The purpose of the approaches outlined here is to slow down / block
            // producers so that consumers have time to process the data sent to them

            Channel<int> channelQueue = Channel.CreateBounded<int>(1);
            var task1 = Task.Run(() => { return ChannelConsumer(channelQueue); });
            var task2 = Task.Run(() => { return ThrottledChannelProducer(channelQueue); });
            await Task.WhenAll(task1, task2);

            var bufferBlockQueue = new BufferBlock<int>(new DataflowBlockOptions { BoundedCapacity = 1 });
            task1 = Task.Run(() => { return BufferBlockConsumer(bufferBlockQueue); });
            task2 = Task.Run(() => { return ThrottledBufferBlockProducer(bufferBlockQueue); });
            await Task.WhenAll(task1, task2);

            var nitoQueue = new AsyncProducerConsumerQueue<int>(maxCount: 1);
            task1 = Task.Run(() => { return NitoConsumer(nitoQueue); });
            task2 = Task.Run(() => { return ThrottledNitoProducer(nitoQueue); });
            await Task.WhenAll(task1, task2);

            var queue = new BlockingCollection<int>(boundedCapacity: 1);
            task1 = Task.Run(() => { return BlockingCollectionConsumer(queue); });
            task2 = Task.Run(() => { return ThrottledBlockingCollectionProducer(queue); });
            await Task.WhenAll(task1, task2);
        }
        async Task ThrottledChannelProducer(Channel<int> queue )
        {
            ChannelWriter<int> writer = queue.Writer;

            // This Write completes immediately.
            await writer.WriteAsync(7);
            Console.WriteLine("Channel wrote 7");

            // This Write (asynchronously) waits for the 7 to be removed
            // before it enqueues the 13.
            Console.WriteLine("Channel asynchronously waiting for 7 to be read before sending 13");
            await writer.WriteAsync(13);
            Console.WriteLine("Channel wrote 13");

            writer.Complete();
            Console.WriteLine("Completed buffer block");
        }

        async Task ChannelConsumer(Channel<int> queue)
        {
            ChannelReader<int> reader = queue.Reader;

            await foreach (int value in reader.ReadAllAsync())
            {
                Console.WriteLine($"Channel consumer read: {value}");
            }
        }

        async Task ThrottledBufferBlockProducer(BufferBlock<int> queue)
        {

            // This Send completes immediately.
            await queue.SendAsync(7);
            Console.WriteLine("Buffer Block wrote 7");

            // This Send (asynchronously) waits for the 7 to be removed
            // before it enqueues the 13.
            Console.WriteLine("Buffer asynchronously waiting for 7 to be read before sending 13");
            await queue.SendAsync(13);
            Console.WriteLine("Buffer Block wrote 13");

            queue.Complete();
            Console.WriteLine("Completed buffer block");
        }

        async Task BufferBlockConsumer(BufferBlock<int> queue)
        {
            while (await queue.OutputAvailableAsync())
            {
                Console.WriteLine($"Buffer block consumer read: {await queue.ReceiveAsync()}");
            }
        }

        async Task ThrottledNitoProducer(AsyncProducerConsumerQueue<int> queue)
        {

            // This Enqueue completes immediately.
            await queue.EnqueueAsync(7);
            Console.WriteLine("Nito queue wrote 7");


            // This Enqueue (asynchronously) waits for the 7 to be removed
            // before it enqueues the 13.
            Console.WriteLine("Nito queue asynchronously waiting for 7 to be read before sending 13");
            await queue.EnqueueAsync(13);
            Console.WriteLine("Nito queue wrote 13");

            queue.CompleteAdding();
            Console.WriteLine("Completed Nito queue ");
        }

        
        async Task NitoConsumer(AsyncProducerConsumerQueue<int> asyncQueue)
        {
            while (await asyncQueue.OutputAvailableAsync())
            {
                Console.WriteLine($"Nito queue consumer read: {await asyncQueue.DequeueAsync()}");
            }
        }

        Task ThrottledBlockingCollectionProducer(BlockingCollection<int> queue)
        {
            // This Add completes immediately.
            queue.Add(7);
            Console.WriteLine("Blocking Collection wrote 7");

            // This Add waits for the 7 to be removed before it adds the 13.
            Console.WriteLine("Blocking collection waiting for 7 to be read before sending 13");
            queue.Add(13);
            Console.WriteLine("Blocking collection wrote 13");

            queue.CompleteAdding();
            Console.WriteLine("Completed blocking collection");

            return Task.CompletedTask;
        }

        Task BlockingCollectionConsumer(BlockingCollection<int> queue)
        {
            foreach(var value in queue.GetConsumingEnumerable())
            {
                Console.WriteLine($"Blocking collection read: {value}");

            }
            return Task.CompletedTask;
        }
    }
}

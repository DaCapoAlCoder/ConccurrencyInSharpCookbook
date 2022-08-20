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
    public class Chapter09_08 : IChapterAsync
    {
        Channel<int> _queueLooped = Channel.CreateUnbounded<int>();
        public async Task Run()
        {

            // There is no queue with an async API in .net. Channels come from a NuGet package
            Channel<int> queueAsyncForeach = Channel.CreateUnbounded<int>();
            var task1 = Task.Run(() => { return ChannelProducer(queueAsyncForeach); });
            var task2 = Task.Run(() => { return ConsumerUsingAwait(queueAsyncForeach); });
            await Task.WhenAll(task1, task2);

            Channel<int> queueLooped = Channel.CreateUnbounded<int>();
            task1 = Task.Run(() => { return ChannelProducer(queueLooped); });
            task2 = Task.Run(() => { return ConsumerUsingOlderTechnique(queueLooped); });
            await Task.WhenAll(task1, task2);

            BufferBlock<int> bufferblock = new();
            await BufferBlockProducer(bufferblock);
            await BufferBlockSingleConsumer(bufferblock);

            bufferblock = new();
            await BufferBlockProducer(bufferblock);
            await BufferBlockMultipleConsumer(bufferblock);
            await BufferBlockMultipleConsumer(bufferblock);

            // This is an async queue developed in Stephen Cleary's own
            // library but it behaves similar to the buffer block method.
            AsyncProducerConsumerQueue<int> asyncQueue = new();
            await NitoAsyncProducer(asyncQueue);
            await NitoAsyncSingleConsumer(asyncQueue);

            asyncQueue = new();
            await NitoAsyncProducer(asyncQueue);
            await NitoAsyncMultipleConsumers(asyncQueue);
            await NitoAsyncMultipleConsumers(asyncQueue);

            // Channels are the recommended way to solve a non blocking producer/consumer
            // requirement. However if the code structure reflects a pipeline, buffer blocks
            // would be a suitable candidate instead. The Nito implementation is only of use
            // if other Nito tools are already in use.
        }

        async Task ChannelProducer(Channel<int> queue)
        {
            // Producer code
            ChannelWriter<int> writer = queue.Writer;
            await writer.WriteAsync(7);
            await writer.WriteAsync(13);
            writer.Complete();
        }

        async Task ConsumerUsingAwait(Channel<int> queue)
        {

            // Consumer code
            // Displays "7" followed by "13".
            ChannelReader<int> reader = queue.Reader;

            // await here would allow the current thread to return
            // to the thread-pool and the UI to remain responsive in
            // the case of a desktop application. Blocking collections
            // would cause the UI to freeze while waiting for data to
            // come in.
            await foreach (int value in reader.ReadAllAsync())
            {
                Console.WriteLine(value);
            }
        }

        async Task ConsumerUsingOlderTechnique(Channel<int> queue)
        {

            // Consumer code (older platforms without await foreach)
            // Displays "7" followed by "13".
            ChannelReader<int> reader = queue.Reader;

            // Waits until there is an item to read or the queue has complete
            // called
            while (await reader.WaitToReadAsync())
            {
                // Tries to read a value from the queue, if there's nothing
                // left then it will cause the loop to exit
                while (reader.TryRead(out int value))
                {
                    Console.WriteLine(value);
                }
            }
        }

        async Task BufferBlockProducer(BufferBlock<int> queue)
        {
            // Producer code
            await queue.SendAsync(7);
            await queue.SendAsync(13);
            queue.Complete();
        }


        async Task BufferBlockSingleConsumer(BufferBlock<int> queue)
        {
            // Consumer code
            // Displays "7" followed by "13".
            // This way of receiving the values in the buffer/queue can show
            // throw if there are multiple threads consuming. Two consumers
            // could read a single output as being available causing a race
            // condition where one consumer would fail to receive and then throw.
            while (await queue.OutputAvailableAsync())
            {
                Console.WriteLine(await queue.ReceiveAsync());
            }
        }
        async Task BufferBlockMultipleConsumer(BufferBlock<int> queue)
        {
            // Consumer code
            // Displays "7" followed by "13".
            // This way of receiving the values allows multiple consumers 
            // to try and receive a value and will fault if the source
            // completes 
            while (true)
            {
                int item;
                try
                {
                    item = await queue.ReceiveAsync();
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("Buffer block completed");
                    break;
                }
                Console.WriteLine(item);
            }
        }

        // The Nito tools operate similarly to the buffer block and require the same
        // kind of configuration with multiple consumers for the same reasons.
        async Task NitoAsyncProducer(AsyncProducerConsumerQueue<int> asyncQueue)
        {
            // Producer code
            await asyncQueue.EnqueueAsync(7);
            await asyncQueue.EnqueueAsync(13);
            asyncQueue.CompleteAdding();
        }

        async Task NitoAsyncMultipleConsumers(AsyncProducerConsumerQueue<int> asyncQueue)
        {
            while (true)
            {
                int item;
                try
                {
                    item = await asyncQueue.DequeueAsync();
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("Nito Async  Queue Completed");
                    break;
                }
                Console.WriteLine(item);
            }
        }

        async Task NitoAsyncSingleConsumer(AsyncProducerConsumerQueue<int> asyncQueue)
        {
            // Consumer code
            // Displays "7" followed by "13".
            while (await asyncQueue.OutputAvailableAsync())
            {
                Console.WriteLine(await asyncQueue.DequeueAsync());
            }
        }
    }
}

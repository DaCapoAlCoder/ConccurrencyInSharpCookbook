using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Chapter09
{
    public class Chapter09_10 : IChapterAsync
    {
        public async Task Run()
        {
            // This arrangement allows items to be dropped when you don't care about processing
            // all items that are sent to the consumer. This prevents the queue from growing beyond
            // system resources
            Channel<int> queue = Channel.CreateBounded<int>(
                new BoundedChannelOptions(1)
                {
                    FullMode = BoundedChannelFullMode.DropOldest,
                });

            var task1 = Task.Run(() => { return ChannelProducer(queue); });
            // The one second delay allows both numbers to be added to the channel 
            // The oldest will be discarded in this configuration meaning 7 is discarded
            // and 13 is read by the consumer
            var task2 = Task.Run(() => { return ChannelConsumer(queue, 1000); });
            await Task.WhenAll(task1, task2);


            queue = Channel.CreateBounded<int>(
                new BoundedChannelOptions(1)
                {
                    FullMode = BoundedChannelFullMode.DropWrite,
                });

            task1 = Task.Run(() => { return ChannelProducer(queue); });
            // The one second delay allows both numbers to be added to the channel 
            // The newest will be discarded in this configuration meaning 13 is discarded
            // and 7 is read by the consumer
            task2 = Task.Run(() => { return ChannelConsumer(queue, 1000); });
            await Task.WhenAll(task1, task2);
        }

        async Task ChannelProducer(Channel<int> queue)
        {
            ChannelWriter<int> writer = queue.Writer;

            // This Write completes immediately.
            await writer.WriteAsync(7);

            // This Write also completes immediately.
            await writer.WriteAsync(13);

            writer.Complete();
        }

        async Task ChannelConsumer(Channel<int> queue, int delay)
        {
            await Task.Delay(delay);

            ChannelReader<int> reader = queue.Reader;

            await foreach (int value in reader.ReadAllAsync())
            {
                Console.WriteLine($"Channel consumer read: {value}");
            }
        }
    }
}

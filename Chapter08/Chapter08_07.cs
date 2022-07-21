using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Chapter08
{
    public class Chapter08_07 : IChapterAsync
    {
        public async Task Run()
        {
            await Test();
            var multiplyBlock = new TransformBlock<int, int>(item => item * 2);
            await GetValuesAsync().WriteToBlockAsync(multiplyBlock);
            bool yes = await multiplyBlock.TryReceiveAll(out var items);
        }
        async Task Test()
        {
            // The idea is to adapt a dataflow block to an async stream (asynchronously yieldable loop)
            // so that the data flow block can be consumed in a part of the application that uses a stream
            var multiplyBlock = new TransformBlock<int, int>(value => value * 2);

            multiplyBlock.Post(5);
            multiplyBlock.Post(2);
            multiplyBlock.Complete();

            // This consumes the b
            await foreach (int item in multiplyBlock.ReceiveAllAsync())
            {
                Console.WriteLine(item);
            }
        }

        async IAsyncEnumerable<int> GetValuesAsync()
        {
            await Task.Delay(1000); // some asynchronous work
            yield return 10;
            await Task.Delay(1000); // more asynchronous work
            yield return 13;
        }
    }

    public static class DataflowExtensions
    {
        // This extension method is used just to get the data from the data flow block
        // its not absolutely necessary and is more just enacpsulation
        public static bool TryReceiveItem<T>(this ISourceBlock<T> block, out T value)
        {
            if (block is IReceivableSourceBlock<T> receivableSourceBlock)
                return receivableSourceBlock.TryReceive(out value);

            try
            {
                value = block.Receive(TimeSpan.Zero);
                return true;
            }
            catch (TimeoutException)
            {
                // There is no item available right now.
                value = default;
                return false;
            }
            catch (InvalidOperationException)
            {
                // The block is complete and there are no more items.
                value = default;
                return false;
            }
        }
        public static async IAsyncEnumerable<T> ReceiveAllAsync<T>(this ISourceBlock<T> block,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // This loop continues while there is data in the data flow block. This keeps the 
            // stream open. Data may flow through the data block, but the block might not be
            // complete. This means there is more data to come. This loop will end if complete
            // is called on the block and there is no data remaining in the pipeline
            while (await block.OutputAvailableAsync(cancellationToken).ConfigureAwait(false))
            {
                // This loop will continue iterating while it can retrieve items, if there
                // are no items left to read, but the block has not had complete called, the outer
                // loop will iterate back here and this line will wait for new data to come in
                while (block.TryReceiveItem(out var value))
                {
                    // yield back to the caller to make the stream
                    yield return value;
                }
            }
        }
    }
    public static class AsyncEnumerableExtensions
    {
        public static async Task WriteToBlockAsync<T>(this IAsyncEnumerable<T> enumerable,
            ITargetBlock<T> block, CancellationToken token = default)
        {
            try
            {
                await foreach (var item in enumerable.WithCancellation(token).ConfigureAwait(false))
                {
                    await block.SendAsync(item, token).ConfigureAwait(false);
                }
                block.Complete();
            }
            catch (Exception ex)
            {
                block.Fault(ex);
            }
        }
    }
}

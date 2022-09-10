using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Chapter10
{
    public class Chapter10_07 : IChapterAsync
    {

        public async Task Run()
        {
            CancellationTokenSource cts = new();
            var customBlock = CreateMyCustomBlock(cts.Token);

            await customBlock.SendAsync(10);
            await customBlock.SendAsync(22);
            Console.WriteLine("Added two values: 10 and 22");
            Console.WriteLine($"Reading first value: {await customBlock.ReceiveAsync()}");
            Console.WriteLine("Cancelling the block");
            cts.Cancel();
            // Although can still read the data here, cancelling does cause the block to drop data, so if it occurs while
            // the block is executing data loss will happen
            Console.WriteLine($"Can still read the second value after cancelling: {await customBlock.ReceiveAsync()}");

            try
            {
                await customBlock.SendAsync(11);
                Console.WriteLine($"Can still write to the block, although it doesn't do anything");
                Console.WriteLine($"Attempting to read the third value:");
                await customBlock.ReceiveAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to read after cancellation with a {ex.GetType()}");
            }
            customBlock.Complete();
        }

        IPropagatorBlock<int, int> CreateMyCustomBlock(
            CancellationToken cancellationToken)
        {
            // All blocks here are cancelled with the token. But it would be possible
            // to cancel the first and send a custom error down the pipeline
            var blockOptions = new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken
            };
            var multiplyBlock = new TransformBlock<int, int>(item => item * 2,
                blockOptions);
            var addBlock = new TransformBlock<int, int>(item => item + 2,
                blockOptions);
            var divideBlock = new TransformBlock<int, int>(item => item / 2,
                blockOptions);

            var flowCompletion = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };
            multiplyBlock.LinkTo(addBlock, flowCompletion);
            addBlock.LinkTo(divideBlock, flowCompletion);

            return DataflowBlock.Encapsulate(multiplyBlock, divideBlock);
        }

    }
}

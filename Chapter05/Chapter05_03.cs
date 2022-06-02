using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Chapter05
{
    public class Chapter05_03 : IChapterAsync
    {
        public async Task Run()
        {
            // unlinking can be used to change filter dynamically
            // It can also be used to pause a data flow mesh
            // There is a race condition in terms of data processing and the disposal/unlinking
            // There won't be any duplication of data or data loss
            // Either that data will flow through the block or it won't
            await Test();
        }

        async Task Test()
        {
            var multiplyBlock = new TransformBlock<int, int>(item => item * 2);
            var subtractBlock = new TransformBlock<int, int>(item => item - 2);

            // keep the IDisposable and dispose of it to unlink the block
            // doesn't really say what happens if there is more than one link
            IDisposable link = multiplyBlock.LinkTo(subtractBlock);
            multiplyBlock.Post(2);
            multiplyBlock.Post(3);

            using CancellationTokenSource cts = new();
            var valuesBeforeUnlink = subtractBlock.ReceiveAllAsync(cts.Token);
            int i = 0;

            try
            {
                await foreach (int value in valuesBeforeUnlink)
                {
                    if (++i == 2)
                    {
                        cts.Cancel();
                    }
                    Console.WriteLine($"Value before unlinking {value}");
                }

            }
            catch (TaskCanceledException)
            {
            }

            // Unlink the blocks.
            // The data posted above may or may not have already gone through the link.
            // In real-world code, consider a using block rather than calling Dispose.
            link.Dispose();

            multiplyBlock.Post(4);
            multiplyBlock.Complete();
            int valueAfterUnlink = await multiplyBlock.ReceiveAsync(); 
            Console.WriteLine($"The value after unlinking {valueAfterUnlink}");
            await multiplyBlock.Completion;
        }
    }
}

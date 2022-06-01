using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Chapter05
{
    public class Chapter05_03 : IChapterAsync
    {
        public async Task Run()
        {
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

            multiplyBlock.Complete();
            await multiplyBlock.Completion;
            var valuesBeforeUnlink = subtractBlock.ReceiveAllAsync();
            await foreach(int value in valuesBeforeUnlink )
            {
                Console.WriteLine($"Value before unlinking {value}");
            }

            

            // Unlink the blocks.
            // The data posted above may or may not have already gone through the link.
            // In real-world code, consider a using block rather than calling Dispose.
            link.Dispose();

            multiplyBlock.Post(4);
            int valueAfterUnlink = await multiplyBlock.ReceiveAsync(); 
            Console.WriteLine($"The value after unlinking {valueAfterUnlink}");
        }
    }
}

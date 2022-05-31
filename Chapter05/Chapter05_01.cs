using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Chapter05
{
    public class Chapter05_01 : IChapterAsync
    {
        public async Task Run()
        {
            LinkBlocks();
            await LinkBlocksPropegateCompletion();
        }
        void LinkBlocks()
        {
            // Once linked data will flow from the source block to the target block
            var multiplyBlock = new TransformBlock<int, int>(item => item * 2);
            var subtractBlock = new TransformBlock<int, int>(item => item - 2);

            // After linking, values that exit multiplyBlock will enter subtractBlock.
            multiplyBlock.LinkTo(subtractBlock);
        }

        async Task LinkBlocksPropegateCompletion()
        {
            var multiplyBlock = new TransformBlock<int, int>(item => item * 2);
            var subtractBlock = new TransformBlock<int, int>(item => item - 2);

            // By default Dataflow blocks only propagate data and not errors nor completions
            // This option allows completions to be propagated forward
            // Propagate completion option will propagate data as well as completions

            // Propagate Completion will also propagate errors which will be wrapped
            // in an AggregateException

            var options = new DataflowLinkOptions { PropagateCompletion = true };
            // Link to (another overload than the one below) can take a predicate that defines a filter
            // A filter determines what data is passed to the linked block
            // If a filter conditions are not met the data is not dropped but another block is found
            // instead to pass the data into. If it can't find a block the data stays in the block,
            // the block will then become stalled until that issues is resolved
            multiplyBlock.LinkTo(subtractBlock, options);

            // ...

            // If data is sent and receive is not awaited calling complete and awaiting 
            // completion just causes the program to stall. Perhaps data is "stuck" in the block
            await multiplyBlock.SendAsync(10);


            // The first block's completion is automatically propagated to the second block.
            multiplyBlock.Complete();

            // Must receive data from the final block. Trying this on Multiply caused an error
            int output = await subtractBlock.ReceiveAsync();
            Console.WriteLine(output);

            // Once the data is received (or no data is sent) the completion can be awaited
            await subtractBlock.Completion;
        }
    }
}

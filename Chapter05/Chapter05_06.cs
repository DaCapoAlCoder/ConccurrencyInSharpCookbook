using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Chapter05
{
    public class Chapter05_06 : IChapterAsync
    {
        public async Task Run()
        {
            // An encapsulated set of blocks must have both a single input block and a single output block
            // in order to use the Encapsulate method
            // Multi input/output meshes that are to be encapsulated require a custom class which expsoes
            // ITargetBlock<T> for the inputs and IReceivableSourceBlock<T> for the outputs
            // Not all block options may make sense to an encapsualted set of blocks, so a custom options
            // class can be created and passed in if required

            // Encapsulates a multiple blocks into a single data flow block - good for re-use
            var dataFlowBlock = CreateMyCustomBlock();
            await dataFlowBlock.SendAsync(10);
            Console.WriteLine(await dataFlowBlock.ReceiveAsync());
            dataFlowBlock.Complete();
            await dataFlowBlock.Completion;
        }

        // Propagator blocks are both data targets and a source of data
        public IPropagatorBlock<int, int> CreateMyCustomBlock()
        {
            var multiplyBlock = new TransformBlock<int, int>(item => item * 2);
            var addBlock = new TransformBlock<int, int>(item => item + 2);
            var divideBlock = new TransformBlock<int, int>(item => item / 2);

            // Creates a a data flow from multiply to add to divide
            var flowCompletion = new DataflowLinkOptions { PropagateCompletion = true };
            multiplyBlock.LinkTo(addBlock, flowCompletion);
            addBlock.LinkTo(divideBlock, flowCompletion);

            // Encapsulate the full data flow into a single block. 
            return DataflowBlock.Encapsulate(multiplyBlock, divideBlock);
        }
    }
}

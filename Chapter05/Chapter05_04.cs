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
    public class Chapter05_04 : IChapterAsync
    {
        public async Task Run()
        {
            await LoadShareWorkWithBoundedCapacity();
        }
        async Task LoadShareWorkWithBoundedCapacity()
        {
            var sourceBlock = new BufferBlock<int>();
            // When a block outputs data it tries to pass the data to each of its connected links
            // Each block will buffer the data until the block is ready to processes it
            // This means that if two blocks are linked to a single source the first block
            // will buffer and take all of the work
            // By bounding the capacity the work can be shared across more than one block

            // This does result in a race condition so work is not split in half between two blocks
            var options = new DataflowBlockOptions { BoundedCapacity = 1 };

            var targetBlockA = new BufferBlock<int>(options);
            var targetBlockB = new BufferBlock<int>(options);
            var multiplyBlockA = new TransformBlock<int, int>(item =>  item * 2);
            var multiplyBlockB = new TransformBlock<int, int>(item =>  item * 4);


            // Link the blocks and propegate completion from the source
            sourceBlock.LinkTo(targetBlockA, new DataflowLinkOptions { PropagateCompletion = true }); ;
            sourceBlock.LinkTo(targetBlockB, new DataflowLinkOptions { PropagateCompletion = true });
            
            targetBlockA.LinkTo(multiplyBlockA, new DataflowLinkOptions { PropagateCompletion = true });
            targetBlockB.LinkTo(multiplyBlockB, new DataflowLinkOptions { PropagateCompletion = true });

            for (int i = 0; i < 10; i++)
            {
                await sourceBlock.SendAsync(2);
            }

            // Mark the head of the pipeline as complete
            // The Completion will propegate
            sourceBlock.Complete();
            await sourceBlock.Completion;

            bool aAvailable = true;
            bool bAvailable = true;
            while(aAvailable || bAvailable)
            {
                //These will await indfinitely unless the completion has propegated
                // through the pipeline
                aAvailable = await multiplyBlockA.OutputAvailableAsync();
                bAvailable = await multiplyBlockB.OutputAvailableAsync();

                if(aAvailable)
                {
                    int a = await multiplyBlockA.ReceiveAsync();
                    Console.WriteLine(a);
                }
                if(bAvailable)
                {
                    int b = await multiplyBlockB.ReceiveAsync();
                    Console.WriteLine(b);
                }
            }
        }
    }
}

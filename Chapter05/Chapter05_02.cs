using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Chapter05
{
    public class Chapter05_02 : IChapterAsync
    {
        public async Task Run()
        {
            DoesNotPropegateThrowException();
            await PropegateAndCatchException();
            await PropegateCompletionAndErrorsOption();
        }

        public void DoesNotPropegateThrowException()
        {
            // The first post to the block will put it in a faulted state
            // the second post data is just dropped and nothing comes out of the block
            // This code does sort of execute. It looks like an async method executing without an await
            var block = new TransformBlock<int, int>(item =>
            {
                if (item == 1)
                    throw new InvalidOperationException("Fault.");
                return item * 2;
            });
            block.Post(1);
            block.Post(2);

            // For simpler data flow pipelines it can be easier to let errors propagate through the pipeline
            // and catch them at the end. It is possible to allow errors to get passed through the pipeline 
            // as another piece of data, allowing them to continue processing even if exceptions occur. Doing
            // this prevents the data blocks from faulting due to error and keep processing.
        }

        // Note this method triggers the above exception too
        async Task PropegateAndCatchException()
        {
            try
            {
                var block = new TransformBlock<int, int>(item =>
                {
                    if (item == 1)
                        throw new InvalidOperationException("Fault.");
                    return item * 2;
                });
                block.Post(1);
                // In order for the block to propagate the exception the Completion must be awaited
                // The completion will return a Task that will complete if the block also completes
                // It will return a faulted task if the block also faults
                await block.Completion;
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("The exception was caught");
            }
        }

        async Task PropegateCompletionAndErrorsOption()
        {
            try
            {
                var multiplyBlock = new TransformBlock<int, int>(item =>
                {
                    if (item == 1)
                        throw new InvalidOperationException("Fault.");
                    return item * 2;
                });

                var subtractBlock = new TransformBlock<int, int>(item =>
                {
                    return item - 2;
                });

                multiplyBlock.LinkTo(subtractBlock,
                    new DataflowLinkOptions { PropagateCompletion = true });

                multiplyBlock.Post(1);

                await subtractBlock.Completion;
            }
            catch (AggregateException ex)
            {
                // Each block can wrap an error in an aggregate
                // even if it is already wrapped in an aggregate.
                // So the Flatten() method can be useful to sort that ou
                Console.WriteLine(ex.Flatten().Message);
            }
        }

    }
}

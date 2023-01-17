using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Chapter01
{
    public class Chapter01_11 : IChapter
    {
        public void Run()
        {
            // Task parallel library (TPL) Data flow blocks are useful when there is
            // a sequence of process that need to be applied to data, such as download
            // it from a URL and process it in parallel along with other data. TPL 
            // dataflow is commonly used as a pipeline, but can also be used for meshes
            // that can handle forks, joins and loops. 

            // If a block faults, it stops receiving data, but the data can be rerouted
            // dynamically through the mesh during runtime. This is an advanced scenario,
            // and in most cases the fault should just be set to propagate down the pipeline.
            // Faults end up in an AggregateException making them a bit more awkward to handle
            // and can sometimes become deeply nested. The exceptions can be flattened with an
            // extension method however.

            // Dataflow seems similar to observable streams and in some cases they are. Observables
            // are better for timing related scenarios and dataflow is better at handling parallel
            // processing

            // TPL is similar to an actor framework in that it will spin up blocks to do work as 
            // needed and can spin up multiple blocks to handle work in parallel. Unlike an actor
            // framework there is no clean error recovery or retries. So it has an actor like feel
            // but is not a fully fledged actor framework.

            Test();
        }

        void Test()
        {
            try
            {
                // Transform blocks act like a linq Select statement to modify
                // data
                var multiplyBlock = new TransformBlock<int, int>(item =>
                {
                    if (item == 1)
                    {
                        throw new InvalidOperationException("Blah.");
                    }

                    return item * 2;
                });

                var subtractBlock = new TransformBlock<int, int>(item => item - 2);

                // Blocks are linked together
                multiplyBlock.LinkTo(subtractBlock,
                    new DataflowLinkOptions { PropagateCompletion = true });

                // Data is send through the pipeline of two blocks
                multiplyBlock.Post(1);

                // Wait synchronously for the data to come out the other end
                // of the pipeline. It would probably be better to await this
                // rather than synchronously block use Wait()
                subtractBlock.Completion.Wait();
            }
            catch (AggregateException exception)
            {
                AggregateException ex = exception.Flatten();
                Console.WriteLine(ex.InnerException);
            }
        }
    }
}

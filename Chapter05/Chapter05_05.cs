using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Chapter05
{
    public class Chapter05_05 : IChapterAsync
    {
        public async Task Run()
        {
            // One way to deterimine if a block should have parallelism added is to stop
            // the block in a debugger and look at the size of the input queue. If its 
            // larger than exepected some parallelism could be addeded
            Console.WriteLine("Time with ubounded parallelism");
            await Test(DataflowBlockOptions.Unbounded);
            Console.WriteLine("Time with no parallelism");
            await Test(1);
        }
        async Task Test(int maxDegreeOfParallelism)
        {
            var multiplyBlock = new TransformBlock<int, int>(
                async item => { await Task.Delay(500); return item * 2; },
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = maxDegreeOfParallelism,
                });
            var subtractBlock = new TransformBlock<int, int>(item => item - 2);
            multiplyBlock.LinkTo(subtractBlock, new DataflowLinkOptions { PropagateCompletion = true });

            int length = 10;
            Random random = new((int)DateTime.Now.Ticks);
            Stopwatch stopwatch = new();
            stopwatch.Start();
            for (int i = 0; i < length; i++)
            {
                await multiplyBlock.SendAsync(random.Next());
            }
            multiplyBlock.Complete();
            await multiplyBlock.Completion;

            var values = subtractBlock.ReceiveAllAsync().ToListAsync(); ;

            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);
        }
    }
}

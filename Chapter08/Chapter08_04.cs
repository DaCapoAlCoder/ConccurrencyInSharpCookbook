using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter08
{
    public class Chapter08_04 : IChapterAsync
    {
        public async Task Run()
        {
            await Test();
        }
        async Task Test()
        {
            var source = Enumerable.Range(0, 100_000_000);
            Action<int> body = x => { x *= 2; };

            // You have a parallel workload, that shouldn't run on the UI thread.
            // Use Task.Run() to shift the parallel workload threads to the thread pool
            // This only applies to UI work, typically parallel work is not done on the
            // server as the framework is already parallel such asp.net 
            await Task.Run(() => Parallel.ForEach(source, body));
        }
    }
}

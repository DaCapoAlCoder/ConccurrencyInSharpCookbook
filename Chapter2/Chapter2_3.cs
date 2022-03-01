using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter2
{
    public class Chapter2_3
    {
        public async Task Run()
        {
            await CallMyMethodAsync();

        }
        async Task MyMethodAsync(IProgress<double> progress = null)
        {
            bool done = false;
            double percentComplete = 0;
            int i = 0;
            const int max = 3;
            while (!done)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                percentComplete = (i + 1) / (double)max;
                progress?.Report(percentComplete);
                done = ++i >= 3;
            }
        }

        async Task CallMyMethodAsync()
        {
            // <T> should be a value type or an immutable type
            //  - "The calling method is asynchronous and can complete before progress is reported"
            //  - There appears to be a race condition between the async method completing and the reporting event (not sure)
            //  - If there is a race condition then immutable report types will guarantee correct value is read at the event (still not sure)
            var progress = new Progress<double>();
            progress.ProgressChanged += (sender, args) =>
            {
                Console.WriteLine($"Complete: {100*args}%");
            };
            await MyMethodAsync(progress);
        }

    }
}

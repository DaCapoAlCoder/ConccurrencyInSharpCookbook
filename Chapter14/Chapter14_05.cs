using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter14
{
    public class Chapter14_05 : IChapterAsync
    {
        public async Task Run()
        {
            // The idea here is that there is a method that need to use
            // both synchronous and asynchronous versions of an API, but
            // the core logic for consuming both of these versions should 
            // not be duplicated
            await DelayAndReturnAsync();
            DelayAndReturn();

            // Ideally I/O would be at the edge of an architecture and would
            // be asynchronous thus not requiring these kind of workarounds
            // but that's not always reflective of reality or adding async
            // into old code.

            // Having the async code call the synchronous code and vice versa
            // are both anti patterns
        }

        private async Task<int> DelayAndReturnCore(bool sync)
        {
            int value = 100;

            // The logic that should not be duplicated would be in here

            // The call to the (a)synchronous API is made depending
            // on the boolean state passed in by the wrapper methods
           
            // Do some work
            if (sync)
            {
                Console.WriteLine("Executing the synchronous API");
                Thread.Sleep(value); // call synchronous API
            }
            else
            {
                Console.WriteLine("Executing the asynchronous API");
                await Task.Delay(value); // call asynchronous API
            }

            return value;
        }

        // These two methods allow the async and synchronous version of
        // the method to be exposed as separate methods for others to
        // consumer 

        // Asynchronous API
        public Task<int> DelayAndReturnAsync() =>
            DelayAndReturnCore(sync: false);

        // Synchronous API
        public int DelayAndReturn() =>
            // Since this is a synchronous method its safe to use 
            // the GetAwaiter GetResult method of blocking the thread
            // to get the result. This also avoids throwing an aggregate
            // exception. The only reason this can block like this is
            // because the task is complete before it returns, if it
            // ever returns an incomplete task, this execution path
            // could deadlock
            DelayAndReturnCore(sync: true).GetAwaiter().GetResult();
    }
}

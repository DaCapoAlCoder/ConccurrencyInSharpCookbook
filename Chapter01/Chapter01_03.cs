using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter01
{
    public class Chapter01_03 : IChapterAsync
    {
        // Error handling occurs in a natural way similar to synchronous
        // code 
        public Task Run()
        {
            return TrySomethingAsync();
        }

        async Task PossibleExceptionAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            throw new NotSupportedException("This is not a supported thing");
        }

        void LogException(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        async Task TrySomethingAsync()
        {
            // This code can catch the exception the same as if
            // the method was synchronous. The exception itself if
            // not wrapped in an AggregateException or TargetInvocationException 
            try
            {
                await PossibleExceptionAsync();
            }
            catch (NotSupportedException ex)
            {
                LogException(ex);
                throw;
            }
        }
    }
}

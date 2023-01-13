using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter01
{
    public class Chapter01_04 : IChapterAsync
    {
        public Task Run()
        {
            // Exception when thrown are stored in the task itself.
            // The task status is set to complete. Only when the task
            // is awaited the exception propagates
            return TrySomethingAsync();
        }
        async Task PossibleExceptionAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            throw new NotSupportedException("This is not a supported thing");
        }
        void LogException(Exception ex)
        {
            string value = $"Exception: {ex.Message}";
            Console.WriteLine(value);
        }

        async Task TrySomethingAsync()
        {
            // The exception will end up on the Task, not thrown directly.
            Task task = PossibleExceptionAsync();
            Console.WriteLine("The exception has occurred but is not yet thrown");
            await Task.Delay(TimeSpan.FromSeconds(3));

            try
            {
                Console.WriteLine("The task is about to be awaited and the exception will be thrown");
                await Task.Delay(TimeSpan.FromSeconds(3));
                // The Task's exception will be raised here, at the await.
                await task;
            }
            catch (NotSupportedException ex)
            {
                LogException(ex);
                // re-throwing is annoying when running the example
                //throw;
            }
        }
    }
}

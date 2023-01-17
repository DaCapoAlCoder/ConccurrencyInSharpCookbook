using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter02
{
    public class Chapter02_11 : IChapterAsync
    {
        public async Task Run()
        {
            await ConsumingMethodAsync();
            await ConsumingMethodAwaitForConcurrentOperationAsync();
            await ConvertToTaskToAwaitSameTaskMultipleTimes();
            await ConvertToTaskToDoTaskOnlyWork();

            ValueTask<int> MethodAsync() => new ValueTask<int>(13);

            async Task ConsumingMethodAsync()
            {
                Console.WriteLine("A ValueTask can be awaited just like a Task");
                int value = await MethodAsync();
                Console.WriteLine($"The value from the awaited ValueTask is {value}");
            }

            async Task ConsumingMethodAwaitForConcurrentOperationAsync()
            {
                ValueTask<int> valueTask = MethodAsync();
                Console.WriteLine($"The ValueTask has started concurrently");
                // ... // other concurrent work
                Console.WriteLine($"Other concurrent work can be done here");
                int value = await valueTask;
                Console.WriteLine($"The method is awaited and the value is {value}");
                Console.WriteLine($"A value task can be awaited a second time, but the result of awaiting twice are undefined");
                Console.WriteLine($"So don't do it");
            }
            async Task ConvertToTaskToAwaitSameTaskMultipleTimes()
            {

                Console.WriteLine("Converting a ValueTask to Task");
                Task<int> task = MethodAsync().AsTask();
                // ... // other concurrent work
                Console.WriteLine($"Awaiting the task with Id {task.Id}");
                int value = await task;
                Console.WriteLine($"The value is {value}");
                Console.WriteLine($"Awaiting the same task again with Id {task.Id}");
                int anotherValue = await task;
                Console.WriteLine($"The value is of course: {anotherValue}");
            }

            async Task ConvertToTaskToDoTaskOnlyWork()
            {

                Console.WriteLine("Converting two ValueTasks to tasks using AsTask method");
                Task<int> task1 = MethodAsync().AsTask();
                Task<int> task2 = MethodAsync().AsTask();

                Console.WriteLine("WhenAll can now be used on the new Tasks");
                int[] results = await Task.WhenAll(task1, task2);
                Console.WriteLine("Its possible to do other Task only work like await multiple times");

                Console.WriteLine("All tasks have completed");
                for (int i = 0; i < results.Length; i++)
                {
                    Console.WriteLine($"Result of task{i + 1}: {results[i]}");
                }

                Console.WriteLine("Once converted to Task the ValueTask should be discarded and not used again");
                Console.WriteLine("Most code should either await a ValueTask or convert it to a Task");

                // Other properties of ValueTask do not act the same as Task. Unlike Task, Calling Result 
                // does not block the current thread. Most scenarios will only require the use cases covered
                // here. Its important to look up the documentation when using ValueTask in other use cases 
            }
        }
    }
}

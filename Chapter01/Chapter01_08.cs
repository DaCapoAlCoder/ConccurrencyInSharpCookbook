using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter01
{
    public class Chapter01_08 : IChapter
    {
        public void Run()
        {
            // Section 1.6 and 1.7 covers data parallelism where data
            // is processed in parallel. Task parallelism is another type
            // of parallel computation, it doesn't focus just on data processing
            // but other kinds of work, perhaps calling methods or processing 
            // logic through a path of execution. Both types can be used to solve
            // similar problems, so the most apt API should be used for a given 
            // scenario.

            // The Task class can also be used for task parallel operations, using
            // the Task.Run method. The Wait method can be used to Wait for the
            // task to complete and the Result and Exception properties can be used
            // to access to corresponding values. Using Task directly is more complex
            // than using Parallel

            // Where Parallel or Task is used the more independent the methods are the
            // better. Otherwise synchronisation will be required to ensure integrity
            // Where a closure is used, (that is a factory method of sorts, that returns
            // another method such as an Action which allows the returned method to
            // access data from the factory) there can be some hidden sharing of data
            // which can cause synchronisation issues. 

            // TODO read a bit more and do a write up after
            Random rand = new();
            int size = 1000;
            double[] array = new double[size];
            for (int i = 0; i < size; i++)
            {
                array[i] = rand.NextDouble();
            }

            ProcessArray(array);
            
        }
        void ProcessArray(double[] array)
        {
            // Parallel.Invoke performs a fork and join operation, creating the threads
            // then waiting for them both to complete before continuing execution in the
            // caller
            Parallel.Invoke(
                () => ProcessPartialArray(array, 0, array.Length / 2 - 1),
                () => ProcessPartialArray(array, array.Length / 2, array.Length - 1)
            );
        }

        void ProcessPartialArray(double[] array, int begin, int end)
        {
            // CPU-intensive processing...
            Console.WriteLine($"Doing some CPU intensive processing on elements {begin} to {end} in thread {Thread.CurrentThread.ManagedThreadId}");
        }
    }
}

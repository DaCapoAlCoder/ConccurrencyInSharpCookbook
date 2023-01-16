using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter01
{
    public class Chapter01_09 : IChapter
    {
        public void Run()
        {
            Test();

            // Data and Task parallelism use partitioners to dynamically adjust
            // the work across the thread pool. There are queues of work and 
            // each thread will usually pull in work from its own queue, however
            // if one thread has no work it will steal work from another thread's
            // queue. There are multiple adjustments that can be made to how this
            // all works in order to perform optimisation on a parallel codebase
            // although generally the defaults work well. Tasks should not be 
            // too short or too long. Too short means the overhead of partitioning
            // the work into the thread pool is not worth it. Too long means that
            // work cannot be efficiently balanced by the thread pool. As a rule
            // of thumb make tasks short enough to the point that they don't cause
            // performance issues. Performance should degrade if the tasks are too
            // short. PLINQ solves these issues by doing this work automatically
        }

        void Test()
        {
            try
            {
                // All Parallel class exceptions are handled in a the same manner
                // using an AggregateException, since operations on concurrent threads
                // can each throw an exception. 
                Parallel.Invoke(() => { throw new Exception("Exception In Method 1"); },
                    () => { throw new Exception("Exception In Method 2"); });
            }
            catch (AggregateException ex)
            {
                // Aggregate exception has methods to handle each contained exception
                // including ways to flatten the exceptions. Here handle will be called
                // on each exception. Both the exceptions in the first and second Actions
                // are stored in the AggregateException type, meaning it is not a race
                // condition as to which exception gets handles here. The handler will
                // call its Action each exception in the AggregateException
                ex.Handle(exception =>
                {
                    Console.WriteLine($"Handling: {exception.Message}");
                    return true; // "handled"
                });
            }
        }
    }
}

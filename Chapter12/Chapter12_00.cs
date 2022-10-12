using Common;
using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter12
{
    public class Chapter12_00 : IChapterAsync
    {
        public async Task Run()
        {
            // Three conditions must be met to require synchronisation:
            //  - Multiple pieces of code runs concurrently
            //  - The concurrent code is reading or writing the same data
            //  - At least one piece of code is updating (writing) that data

            #region Tasks executing sequentially on thread pool
            await MultiThreadedButSequentialAccess();
            #endregion

            #region Concurrent Tasks modifying field and method local variable on the thread pool
            int value = 0;
            int valueFromPrivateField = 0;
            Console.WriteLine($"Executing from a thread pool");
            for (int i = 0; i < 6; i++)
            {
                Console.WriteLine($"Attempt {i + 1}");
                value = await ModifyValueConcurrentlyAsync();
                valueFromPrivateField = await ModifySharedPrivateFieldConcurrentlyAsync();

                if (value != 3)
                {
                    Console.WriteLine($"Concurrency error, value not as expected {value}");
                }
                else
                {
                    Console.WriteLine($"Value correct");
                }


                if (valueFromPrivateField != 3)
                {
                    Console.WriteLine($"Concurrency error, value from field not as expected {valueFromPrivateField}");
                }
                else
                {
                    Console.WriteLine($"Value from field correct");
                }

                // Reset private field
                _sharedValueInPrivateField = 0;
            }
            #endregion

            #region Concurrent Tasks modifying field with internal reassignment on the thread pool
            Console.WriteLine($"Executing from a synchronisation context");
            for (int i = 0; i < 6; i++)
            {
                Console.WriteLine($"Attempt {i + 1}");
                value = AsyncContext.Run(ModifyValueConcurrentlyAsync);
                valueFromPrivateField = AsyncContext.Run(ModifySharedPrivateFieldConcurrentlyAsync);

                if (value != 3)
                {
                    Console.WriteLine($"Concurrency error, value not as expected {value}");
                }
                else
                {
                    Console.WriteLine($"Value correct");
                }


                if (valueFromPrivateField != 3)
                {
                    Console.WriteLine($"Concurrency error, value from field not as expected {valueFromPrivateField}");
                }
                else
                {
                    Console.WriteLine($"Value from field correct");
                }
                // Reset private field
                _sharedValueInPrivateField = 0;
            }
            #endregion

            #region Concurrent Tasks modifying field with internal reassignment in a synchronisation context
            Console.WriteLine($"Executing from a synchronisation context but assigning locally");
            for (int i = 0; i < 6; i++)
            {
                Console.WriteLine($"Attempt {i + 1}");
                value = AsyncContext.Run(ModifyFieldWithInternalReasignmentConcurrentlyAsync);

                if (value != 3)
                {
                    Console.WriteLine($"Concurrency error, value not as expected {value}");
                }
                else
                {
                    Console.WriteLine($"Value correct");
                }

                // Reset private field
                _sharedValueInPrivateField = 0;
            }
            #endregion

            #region Simple Parallel on Thread Pool
            Console.WriteLine($"Executing from a thread pool");
            for (int i = 0; i < 600; i++)
            {
                Console.WriteLine($"Attempt {i + 1}");
                value = await SimpleParallelismAsync();

                if (value != 3)
                {
                    Console.WriteLine($"Concurrency error, value not as expected {value}");
                    break;
                }
                else
                {
                    Console.WriteLine($"Value correct");
                }

            }
            #endregion

            #region Simple Parallel Synchronisation Context
            Console.WriteLine($"Executing from a synchronisation context");
            for (int i = 0; i < 600; i++)
            {
                Console.WriteLine($"Attempt {i + 1}");
                value = AsyncContext.Run(SimpleParallelismAsync);

                if (value != 3)
                {
                    Console.WriteLine($"Concurrency error, value not as expected {value}");
                    break;
                }
                else
                {
                    Console.WriteLine($"Value correct");
                }
            }
            #endregion

            #region Parallel.Foreach No Shared State
            IndependentParallelism(Enumerable.Range(0, 1000));
            #endregion

            #region Parallel.Foreach With Shared State
            int arraySize = 1_000_000_000;
            var array = new int[arraySize];
            Array.Fill(array, 1);
            value = ParallelSum(array);
            if (arraySize != value)
            {
                Console.WriteLine($"Total incorrect {value}");
            }
            else
            {
                Console.WriteLine($"Total: {value}");
            }
            #endregion

            #region ImmutableStackNoSharedState
            bool isStackEmpty = await PlayWithStackAsync();
            Console.WriteLine($"Is stack empty {isStackEmpty}");
            #endregion

            #region ImmutableStackWithSharedState
            var stack = await PlayWithStackSharedVariableAsync();
            int count = stack.Count();
            if (count != 3)
            {
                Console.WriteLine($"Synchronisation error stack size is {count}");
            }
            else
            {
                Console.WriteLine("Stack count correct");
            }
            #endregion

            #region ThreadSafeCollection
            var dictionaryCount = await ThreadsafeCollectionsAsync();
            if (dictionaryCount != 3)
            {
                Console.WriteLine($"Synchronisation error stack size is {dictionaryCount}");
            }
            else
            {
                Console.WriteLine("Stack count correct");
            }
            #endregion
        }


        async Task MultiThreadedButSequentialAccess()
        {
            // Because this is running on a thread pool and not a synchronisation context,
            // this method can switch executing threads after each await statement. This means
            // different threads are accessing the same value, but synchronisation is not required
            // here because the threads are accessing the value sequentially and not concurrently
            // On a desktop app you may need to use Task.Run on the method to get it to execute on
            // the thread pool
            int value = 10;
            Console.WriteLine($"Thread Id is now: {Thread.CurrentThread.ManagedThreadId}");
            await Task.Delay(TimeSpan.FromSeconds(1));
            Console.WriteLine($"Thread Id is now: {Thread.CurrentThread.ManagedThreadId}");
            value = value + 1;
            await Task.Delay(TimeSpan.FromSeconds(1));
            Console.WriteLine($"Thread Id is now: {Thread.CurrentThread.ManagedThreadId}");
            value = value - 1;
            await Task.Delay(TimeSpan.FromSeconds(1));
            Console.WriteLine($"Thread Id is now: {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine(value);
        }
        class SharedData
        {
            public int Value { get; set; }
        }

        async Task ModifyValueAsync(SharedData data)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            data.Value = data.Value + 1;
        }

        // WARNING: may require synchronization; see discussion below.
        async Task<int> ModifyValueConcurrentlyAsync()
        {
            var data = new SharedData();

            // This executes 3 instances of the method asynchronously and concurrently
            // once the tasks are created there are up to 3 separate threads executing
            // concurrently and modifying a single value. WhenAll will wait for all three
            // to complete. When executing on a thread pool this will cause problem that
            // will require synchronisation. When executing from a synchronisation context that allows
            // only a single thread to execute, the tasks are queued and executed on a single thread
            // and so no synchronisation is required. This could be in old ASP.NET or in the
            // GUI thread of a desktop application

            // Start three concurrent modifications.
            Task task1 = ModifyValueAsync(data);
            Task task2 = ModifyValueAsync(data);
            Task task3 = ModifyValueAsync(data);

            await Task.WhenAll(task1, task2, task3);
            Console.WriteLine(data.Value);
            return data.Value;
        }

        // private field 
        private int _sharedValueInPrivateField;

        async Task ModifyValueAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            _sharedValueInPrivateField = _sharedValueInPrivateField + 1;
        }

        // WARNING: may require synchronization; see discussion below.
        async Task<int> ModifySharedPrivateFieldConcurrentlyAsync()
        {
            // In this case the shared value being modified is a private field
            // in the calling class rather than a class created in the stack
            // of the calling method. But the same synchronisation issues apply

            // Start three concurrent modifications.
            Task task1 = ModifyValueAsync();
            Task task2 = ModifyValueAsync();
            Task task3 = ModifyValueAsync();

            await Task.WhenAll(task1, task2, task3);

            return _sharedValueInPrivateField;
        }


        async Task ModifyValueWithReassignmentAsync()
        {
            // When synchronised should count up to 3
            int originalValue = _sharedValueInPrivateField;
            await Task.Delay(TimeSpan.FromSeconds(originalValue));
            Console.WriteLine($"Before: Original Value: {originalValue}. Shared Value: {_sharedValueInPrivateField}");
            _sharedValueInPrivateField = originalValue + 1;
            Console.WriteLine($"After: Original Value: {originalValue}. Shared Value: {_sharedValueInPrivateField}");
        }

        async Task<int> ModifyFieldWithInternalReasignmentConcurrentlyAsync()
        {
            // Even though these will be called in a synchronisation context
            // where the threads should execute one at a time, anytime the
            // method does an await the value may be changed by another thread.
            // So synchronisation is still required here in spite of the presence
            // of a synchronisation context

            // In this case while a task is being awaited the synchronisation context
            // can allow a new thread in to start work. Since the data is shared
            // between threads, one thread can modify the value, while another is
            // waiting for the task to complete. In this case its a race condition
            // for the method to finish before the next thread starts. The first
            // thread has no delay and modifies the value correctly. The second comes
            // in correctly copies the value then awaits, the third then thread comes
            // in and copies the same value as the second thread, since the second
            // thread has not completed due to awaiting the task. This results in the
            // final value being two where as it would be three if only one thread was
            // allowed to enter the method at a time via synchronisation.

            // Start three concurrent modifications.
            Task task1 = ModifyValueWithReassignmentAsync();
            Task task2 = ModifyValueWithReassignmentAsync();
            Task task3 = ModifyValueWithReassignmentAsync();

            await Task.WhenAll(task1, task2, task3);

            return _sharedValueInPrivateField;
        }

        async Task<int> SimpleParallelismAsync()
        {
            // These tasks are all being created and executed on the thread
            // pool using Task.Run. It doesn't matter if this method is being
            // called from a synchronisation context, they still end up in the
            // thread pool. Even though value is local to the method it is still
            // being shared among threads and requires synchronisation. The tasks
            // all read from and write to a shared piece of data
            int value = 0;
            Task task1 = Task.Run(() => { value = value + 1; });
            Task task2 = Task.Run(() => { value = value + 1; });
            Task task3 = Task.Run(() => { value = value + 1; });
            await Task.WhenAll(task1, task2, task3);
            return value;
        }

        void IndependentParallelism(IEnumerable<int> values)
        {
            // The array is technically being shared among threads here, but
            // it is split out between each of the threads and so no synchronisation
            // is required because no data is being shared between threads
            Parallel.ForEach(values, item => Console.WriteLine($"Value: {item} Thread: {Thread.CurrentThread.ManagedThreadId}"));
        }

        // BAD CODE!!
        int ParallelSum(IEnumerable<int> values)
        {
            // The first two steps are okay, as they initialise a thread specific
            // local variable and then add an enumerable value only accessed by
            // that thread. So multiple threads are not accessing shared data.

            // The third step sums the thread only values into a single result that
            // is shared among all the threads created in Parallel.ForEach and so
            // synchronisation is required here
            int result = 0;
            Parallel.ForEach(source: values,
                // Thread specific local variable is initialised with this method
                localInit: () => 0,
                // This is the operation to carry out (adds the array element to 
                // the thread local variable)
                body: (item, state, localValue) => localValue + item,
                // Once all threads have completed this action is carried out 
                // aggregating all of the local values into a single sum total
                // The purpose is to add all the values in the input array
                localFinally: localValue => { result += localValue; });
            return result;
        }
        async Task<bool> PlayWithStackAsync()
        {
            ImmutableStack<int> stack = ImmutableStack<int>.Empty;

            // Immutable collections don't require synchronisation because they cannot be updated. 
            // Every time a new element is pushed to the stack, a new stack is created
            Task task1 = Task.Run(() => Console.WriteLine($"Pushed {stack.Push(3).Peek()} on to stack"));
            Task task2 = Task.Run(() => Console.WriteLine($"Pushed {stack.Push(5).Peek()} on to stack"));
            Task task3 = Task.Run(() => Console.WriteLine($"Pushed {stack.Push(7).Peek()} on to stack"));
            await Task.WhenAll(task1, task2, task3);

            return stack.IsEmpty; // Always returns true.
        }

        // BAD CODE!!
        async Task<ImmutableStack<int>> PlayWithStackSharedVariableAsync()
        {
            // Synchronisation is required here because the same stack is being
            // assigned to across multiple threads. Doing this implementation 
            // event with synchronisation is likely not the best way to use
            // an immutable stack.
            ImmutableStack<int> stack = ImmutableStack<int>.Empty;

            Task task1 = Task.Run(() => { stack = stack.Push(3); });
            Task task2 = Task.Run(() => { stack = stack.Push(5); });
            Task task3 = Task.Run(() => { stack = stack.Push(7); });

            await Task.WhenAll(task1, task2, task3);

            return stack;
        }

        async Task<int> ThreadsafeCollectionsAsync()
        {
            // Unlike immutable stack that is shared across threads a
            // concurrent dictionary does not require synchronisation
            var dictionary = new ConcurrentDictionary<int, int>();

            Task task1 = Task.Run(() => { dictionary.TryAdd(2, 3); });
            Task task2 = Task.Run(() => { dictionary.TryAdd(3, 5); });
            Task task3 = Task.Run(() => { dictionary.TryAdd(5, 7); });
            await Task.WhenAll(task1, task2, task3);

            return dictionary.Count; // Always returns 3.
        }
    }
}

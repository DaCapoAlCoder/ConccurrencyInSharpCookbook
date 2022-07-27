using Common;
using System;
using System.Collections.Immutable;

namespace Chapter09
{
    public class Chapter09_01 : IChapter
    {
        public void Run()
        {
            ImmutableStackDemo();
            ImmutableStackShowsDifferentReferences();
            // This one is not in the book. This method has most of the use case notes
            DemoShowsMemoryIsSharedBetweenSnapshots();
            ImmutableQueue();

        }
        void ImmutableStackDemo()
        {
            // Stacks are first in first out. Useful for a sequence of operations to perform
            // Immutable stacks don't perform as fast as the non immutable equivalent 
            // Stacks are first in last out 
            ImmutableStack<int> stack = ImmutableStack<int>.Empty;

            // When adding elements it is necessary to operate on the
            // new stack that is returned rather than the original.
            // The stack is immutable so a new one is created
            // with each change and so the variable must be overwritten
            stack = stack.Push(13);
            stack = stack.Push(7);

            // Displays "7" followed by "13".
            foreach (int item in stack)
            {
                Console.WriteLine(item);
            }

            int lastItem;
            stack = stack.Pop(out lastItem);
            Console.WriteLine($"Last item = {lastItem}");
        }

        void ImmutableStackShowsDifferentReferences()
        {
            ImmutableStack<int> stack = ImmutableStack<int>.Empty;
            // Overwrite the original reference with a stack containing 13
            stack = stack.Push(13);

            // Creates a bigger stack containing both 7 and 13
            ImmutableStack<int> biggerStack = stack.Push(7);

            // Displays "7" followed by "13".
            Console.WriteLine("Third reference containing both items:");
            foreach (int item in biggerStack)
            {
                Console.WriteLine(item);
            }

            // Only displays "13".
            // The stack with the first addition of 13 is unchanged 
            // after 7 was pushed into it. The original reference 
            // does not change after an operation. Only the returned
            // collection will contains the most recent values
            Console.WriteLine("Second reference unchanged even after another item was added");
            foreach (int item in stack)
            {
                Console.WriteLine(item);
            }

            // Even though references change on each modification values in the queue still share
            // memory of the contained value. This allows immutability while efficiently sharing
            // snapshots of collection state

            // The
        }

        void ImmutableQueue()
        {
            // Queues are first in first out
            ImmutableQueue<int> queue = ImmutableQueue<int>.Empty;
            queue = queue.Enqueue(13);
            queue = queue.Enqueue(7);

            // Displays "13" followed by "7".
            foreach (int item in queue)
            {
                Console.WriteLine(item);
            }

            int nextItem;
            queue = queue.Dequeue(out nextItem);
            // Displays "13"
            Console.WriteLine(nextItem);
        }

        void DemoShowsMemoryIsSharedBetweenSnapshots()
        {
            // Because the stacks share memory its possible to still mutate a class
            // stored in an immutable stack, this not a good idea. 
            ImmutableStack<TestClass> stack = ImmutableStack<TestClass>.Empty;
            TestClass testclass1 = new();
            TestClass testclass2 = new();
            testclass1.Number = 10;
            var stack1 = stack.Push(testclass1);
            var stack2 = stack1.Push(testclass2);


            // Modify the values in stack two. Not a good idea in real code
            foreach(var tc in  stack2)
            {
                tc.Number++;
            }

            // Get the value from stack1 
            stack1.Pop(out var tc1);

            // The value from stack 1 is modified even though the operation took place
            // on the stack2 snapshot. This shows the memory is shared between stack
            // snapshots. The collection does not reallocate its full contents on each
            // snapshot
            Console.WriteLine(tc1.Number);

            // This demo shows that you can mutate items within a stack. It is
            // not thread-safe to do so. New object should be added and removed not mutated

            // References to immutable collection are also not thread safe. They
            // still need to be used with care in multi-threaded scenarios. 
            // Variables that refer to immutable collection will require synchronisation
            // That means as you push and pop elements the variable holding the
            // returned stack requires synchronisation.

            // A good use case is where we don't care about the current state of
            // the collection but rather want to process a snapshot. A concurrent
            // stack would be more suitable where multiple threads must update
            // and know the current state of the stack. For producer-consumer
            // scenarios concurrent collection is better since the consumer
            // must know about the producers changes. For snapshot or creator
            // processor scenarios the immutable stack is preferred as the  processor
            // doesn't care about changes made by others, once a snapshot is received.

        }

        class TestClass
        {
            public int Number { get; set; }
        }
    }
}

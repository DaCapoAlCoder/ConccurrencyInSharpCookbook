using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter04
{
    public class Chapter04_04 : IChapter
    {
        public void Run()
        {
            BinaryTree bt = new();
            Random random = new();
            for (int i = 0; i < 8; i++)
            {
                bt.Add(random.Next(10_000_000, 99_999_999));
            }

            // Processes each node in a binary tree
            // This is dynamic parallelism where the number of parallel tasks to run
            // is not known until runtime. 

            // Instead of using the parent/child relationship, it would be possible3
            // to store all threads in a thread safe collection and use Task.WhenAll to run
            // them all at once.
            ProcessTree(bt.Root);

            ParallelTaskWithContinuation();
        }

        void DoExpensiveActionOnNode(Node node)
        {
            Random random = new((int)DateTime.Now.Ticks);
            List<int> list = new List<int>();

            for (int i = 0; i < node.Data; i++)
            {
                list.Add(random.Next(1, 99_999_999));
            }
            list.Sort();
        }

        void Traverse(Node current)
        {
            // This was for fun, it will start a new thread to process
            // the node while also creating new threads to process the child
            // nodes in parallel
            //Task.Factory.StartNew(
            //    () => DoExpensiveActionOnNode(current),
            //    CancellationToken.None,
            //    TaskCreationOptions.AttachedToParent,
            //    TaskScheduler.Default);

            // This does the expensive processing on the current node
            // before starting new threads to handle the child nodes
            DoExpensiveActionOnNode(current);

            //If there are child nodes start a thread to handle each
            if (current.LeftNode != null)
            {
                Task.Factory.StartNew(
                    () => Traverse(current.LeftNode),
                    CancellationToken.None,
                    // Links each child task with its parent reflecting the parent/child relationship of the tree
                    // Parents execute their delegate and wait for their child tasks to complete. Exceptions propagate
                    // from child to parent so the whole process tree can be executed by calling wait at the root
                    TaskCreationOptions.AttachedToParent,
                    // Its always a good idea to specify the scheduler
                    TaskScheduler.Default);
            }
            if (current.RightNode != null)
            {
                Task.Factory.StartNew(
                    () => Traverse(current.RightNode),
                    CancellationToken.None,
                    TaskCreationOptions.AttachedToParent,
                    TaskScheduler.Default);
            }
        }

        // Process the tree, by processing the current node,
        // then starting up to two threads to process any available
        // child nodes. So you won't really have more than two threads
        // running at a time. 
        void ProcessTree(Node root)
        {
            Task task = Task.Factory.StartNew(
                () => Traverse(root),
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default);
            // Tasks are used for Parallel and asynchronous execution
            // For parallel work blocking call like Task.Wait() and Task.Result can be used
            // these should not be used for asynchronous work
            task.Wait();
        }

        void ParallelTaskWithContinuation()
        {
            // If there is no parent child relationship between items being processed
            // one task can be scheduled to run after another using a a continuation
            Task task = Task.Factory.StartNew(
                () =>
                {
                    Console.WriteLine($"Initial task in thread: {Thread.CurrentThread.ManagedThreadId}");
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default);
            Task continuation = task.ContinueWith(
                (t) =>
                {
                    Console.WriteLine($"Continuation in thread {Thread.CurrentThread.ManagedThreadId}");
                    Console.WriteLine("Task is done");
                },
                CancellationToken.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);
            continuation.Wait();
            // The "t" argument to the continuation is the same as "task".
        }

        // Literally Copied the first Binary Tree Implementation I found on Google. Only need add
        // Its not very clearly written but its grand for a demo
        // https://csharpexamples.com/c-binary-search-tree-implementation/
        class Node
        {
            public Node LeftNode { get; set; }
            public Node RightNode { get; set; }
            public int Data { get; set; }
        }

        class BinaryTree
        {
            public Node Root { get; set; }

            public bool Add(int value)
            {
                Node before = null, after = this.Root;

                while (after != null)
                {
                    before = after;
                    if (value < after.Data) //Is new node in left tree? 
                        after = after.LeftNode;
                    else if (value > after.Data) //Is new node in right tree?
                        after = after.RightNode;
                    else
                    {
                        //Exist same value
                        return false;
                    }
                }

                Node newNode = new Node();
                newNode.Data = value;

                //Tree is empty
                if (this.Root == null)
                {
                    this.Root = newNode;
                    return true;
                }

                if (value < before.Data)
                {
                    before.LeftNode = newNode;
                    return true;
                }

                before.RightNode = newNode;
                return true;
            }
        }
    }
}

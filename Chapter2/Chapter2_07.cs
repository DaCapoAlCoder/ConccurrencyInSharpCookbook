using Common;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter2
{
    public class Chapter2_07 : IChapterAsync
    {
        public Task Run()
        {
            // Too many continuations on a single context can cause performance issues, (guideline is around 100 or so per second)
            // Configure await false can prevent continuation on the original context if its not necessary. There's also sometihing
            // around using ConfigureAwait false in library code because you don't know where the  code will be executed

            Console.WriteLine($"Hello from calling method with thread {Thread.CurrentThread.ManagedThreadId}");
            // This only works because the AsyncContext library creates a Context for the methods to run in
            // If the CallBoth method is simply awaited here there is no syncrhonization context and so the default
            // context is used which means the methods can resume on any thread on the thread pool
            AsyncContext.Run(CallBoth);
            return Task.CompletedTask;
        }

        async Task CallBoth()
        {
            await ResumeOnContextAsync();
            await ResumeWithoutContextAsync();
        }

        async Task ResumeOnContextAsync()
        {
            // This method resumes within the same context.
            Console.WriteLine($"Begin await in ResumeOnContextAsync with thread ID: {Thread.CurrentThread.ManagedThreadId}");
            await Task.Delay(TimeSpan.FromSeconds(1));
            Console.WriteLine($"Resumed ResumeOnContextAsync with thread ID: {Thread.CurrentThread.ManagedThreadId}");
        }

        async Task ResumeWithoutContextAsync()
        {
            // This method discards its context when it resumes.
            Console.WriteLine($"Begin await ResumeWithoutContextAsync in with thread ID: {Thread.CurrentThread.ManagedThreadId}");
            await Task.Delay(TimeSpan.FromSeconds(1))
                .ConfigureAwait(false);
            Console.WriteLine($"Resumed await in ResumeWithoutContextAsync with thread ID: {Thread.CurrentThread.ManagedThreadId}");
        }
    }
}

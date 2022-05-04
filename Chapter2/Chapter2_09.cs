using Common;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Chapter2
{
    public class Chapter2_09 : IChapterAsync
    {
        public async Task Run()
        {

            // Adding the async Task override of execute allows for more testable code. There is an assumption
            // that without being able to catch exceptions Xunit functions like Record.Exception would not work 
            // on async void methods.
            MyAsyncCommand command1 = new();
            try
            {
                // This calls the async Task method because the type of command1 is MyAsyncCommand
                await command1.Execute("parameter");
            }
            catch
            {
                // The exception for async Task method is actually caught
                Console.WriteLine("Exception caught");
            }

            // This instance calls the async void method because the type of command2 is ICommand
            ICommand command2 = new MyAsyncCommand();
            try
            {
                // The caller of an async void method cannot catch the exception.
                // This inability is a failure in the language, rather than any programming related issue
                //command2.Execute("parameter");
            }
            catch
            {
                // The exception for the async void method is not caught
                Console.WriteLine("This line of code will never execute");
            }

            PretendMainMethodInProgramCs(new string[] { });
        }


        sealed class MyAsyncCommand : ICommand
        {
            async void ICommand.Execute(object parameter)
            {
                // The program ends here when the exception occurs in the async Task method
                await Execute(parameter);
            }

            public async Task Execute(object parameter)
            {
                // Asynchronous command implementation goes here.

                await Task.FromException(new InvalidOperationException());
            }


            // ... // Other members (CanExecute, etc)
            public bool CanExecute(object parameter)
            {
                CanExecuteChanged?.Invoke(null, null);
                throw new NotImplementedException();
            }
            public event EventHandler CanExecuteChanged;
        }

        static void PretendMainMethodInProgramCs(string[] args)
        {
            try
            {
                // UWP, WPF and Asp.Net framework all have synchronization contexts and ways to handle top level exceptions
                //     - Application.UnhandledException
                //     - Application.DispatcherUnhandledException
                //     - UseExceptionHandler middleware
                // respectively. 

                // Setups a synchronization context for the console app
                // Don't set synch context on threads you don't own.  Threads you own
                //     - The main thread in a console app
                //     - Any threads you start yourself
                AsyncContext.Run(() => MainAsync(args));
            }
            catch
            {
                Console.WriteLine("This exception was caught because it was propegated to the syncrhonization context ");
            }
            Console.WriteLine("The code can still execute");
        }

        // BAD CODE!!!
        // In the real world, do not use async void unless you have to.

        //Because there is a syncrhonization context for this method, the exception
        //will propegate to that thread allowing it to be caught and handled
        static async void MainAsync(string[] args)
        {
            // In the debugger execution stops here while the exception gets thrown
            // this doesn't happen in an async Task method which handles an exception which is interesting
            await Task.FromException(new InvalidOperationException());
        }
    }
}

using Common;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter14
{
    public class Chapter14_04 : IChapterAsync
    {
        public async Task Run()
        {
            await DoLongOperationAsync();
            await DoALongOperationAsync();

            // Async static works for synchronous and asynchronous code
            // It can be used to replace the old ThreadStaticAttribute
        }

        private static AsyncLocal<Guid> _operationId = new AsyncLocal<Guid>();

        async Task DoLongOperationAsync()
        {
            // The purpose of the AsyncLocal value is that it allows values to be set in the caller
            // and that value can be exposed to child/called/dependent methods. However if the child
            // updates the value, that value will only be exposed to methods it calls, any parent
            // methods will only see the value they have either been passed down from another parent
            // or have assigned themselves, never the value set by any methods it calls.

            // Apparently this is good for logging, in that it allows you to avoid passing an id parameter
            // in every method. As can be seen here, the method is a class scoped field. So updating the
            // Value can happen within methods without having to directly pass an Id or the field as an argument
            // while still maintaining the ability to retain a value for each async context in the call stack.
            _operationId.Value = Guid.NewGuid();
            Console.WriteLine("In the calling thread the AsyncLocalValue is set to:" + _operationId.Value);

            await DoSomeStepOfOperationAsync();
            Console.WriteLine("Back in the calling method the AsyncLocal value is still has its original value:" + _operationId.Value);
        }

        async Task DoSomeStepOfOperationAsync()
        {
            await Task.Delay(100); // some async work

            Console.WriteLine("In the called method the value is the same as the caller:");
            // Do some logging here.
            Console.WriteLine("In operation: " + _operationId.Value);
            var guid = Guid.NewGuid();
            _operationId.Value = guid;
            Console.WriteLine("In the called method the value has been updated to" + _operationId.Value);
        }

        // The purpose of this class is to allow more complex types to be stored in AsyncLocal
        // The class is required because the data should be immutable and so the class wraps
        // async local to provide an interface that ensures the data is immutable.
        // Since the core backing field is async local it still has the same behaviour. Each
        // method call will have access to the immutable stack of its parent and can add more
        // values to it. As you go back up the calls stack the immutable stack will not have 
        // have values add in child methods.
        internal sealed class AsyncLocalGuidStack
        {
            private readonly AsyncLocal<ImmutableStack<Guid>> _operationIds =
                new AsyncLocal<ImmutableStack<Guid>>();

            private ImmutableStack<Guid> Current =>
                _operationIds.Value ?? ImmutableStack<Guid>.Empty;

            // This private class is returned so that it can be used with
            // a using block since its an IDisposable. Once disposed it
            // will pop the value off of the stack
            public IDisposable Push(Guid value)
            {
                _operationIds.Value = Current.Push(value);
                return new PopWhenDisposed(this);
            }

            private void Pop()
            {
                // When the inner class is disposed this method
                // will be called to pop the value from the stack
                // and reassign the immutable stack
                ImmutableStack<Guid> newValue = Current.Pop();
                if (newValue.IsEmpty)
                {
                    newValue = null;
                }
                _operationIds.Value = newValue;
            }

            public IEnumerable<Guid> Values => Current;

            private sealed class PopWhenDisposed : IDisposable
            {
                private AsyncLocalGuidStack _stack;

                // This is the outer class being passed in to this
                // constructor
                public PopWhenDisposed(AsyncLocalGuidStack stack)
                {
                    _stack = stack;
                }

                // This will pop the value from the stack held within
                // the outer class
                public void Dispose()
                {

                    // If two calls are made to push, it only pops off one
                    // leaving the other in there on dispose. The stack is 
                    // set to null though so that cleans it up, but its doesn't
                    // seem great. It could be solved with a for loop, but the 
                    // use case for auto popping in this manner would need to be
                    // examined
                    _stack?.Pop();
                    _stack = null;
                }
            }
        }

        private static AsyncLocalGuidStack _operationIds = new AsyncLocalGuidStack();

        async Task DoALongOperationAsync()
        {
            using (_operationIds.Push(Guid.NewGuid()))
            {
                await DoAStepOfOperationAsync();
                foreach (var val in _operationIds.Values)
                {
                    Console.WriteLine($"values after exiting the inner method {val}");
                }
            }

           Console.WriteLine($"Number values after exiting using block: {_operationIds.Values.Count()}");
        }

        async Task DoAStepOfOperationAsync()
        {
            await Task.Delay(100); // some async work

            // Do some logging here.
            Console.WriteLine("In operation: " +
                string.Join(":", _operationIds.Values));

            var secondGuid = Guid.NewGuid();
            Console.WriteLine($"Adding a second GUID in inner method {secondGuid}");
            _operationIds.Push(secondGuid);

            foreach (var val in _operationIds.Values)
            {
                Console.WriteLine($"All values in the inner method {val}");
            }
        }
    }
}

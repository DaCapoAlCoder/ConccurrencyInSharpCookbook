using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;

namespace Chapter07
{
    [TestClass]
    public class Chapter07_01
    {
        class Sut
        {
            public Task<bool> MyMethodAsync(bool returnVal = false) => Task.FromResult(returnVal);

            public Task<bool> ThrowException() => throw new InvalidOperationException();
        }


        [TestMethod]
        public async Task MyMethodAsync_ReturnsFalseAsync()
        {
            // If the test framework cannot use async Task
            // use GetAwaiter().GetResult() to run the test 
            // synchronously. Its meant to be better than Wait() for exceptions
            // but examining that showed they both acted the same when throwing exceptions
            var objectUnderTest = new Sut();
            bool result = await objectUnderTest.MyMethodAsync();
            Assert.IsFalse(result);
        }

        [TestMethod]
        // Uses NitoAsync.Ex nuget package to provide an async context to run
        // async code within a synchronous method. Better than GetAwaiter.GetResult
        public void MyMethodAsync_ReturnsFalseSynchronous()
        {
            AsyncContext.Run(async () =>
            {
                var objectUnderTest = new Sut();
                bool result = await objectUnderTest.MyMethodAsync();
                Assert.IsFalse(result);
            });
        }

        interface IMyInterface
        {
            Task<int> SomethingAsync();
        }

        class SynchronousSuccess : IMyInterface
        {
            public Task<int> SomethingAsync()
            {
                return Task.FromResult(13);
            }
        }

        class SynchronousError : IMyInterface
        {
            public Task<int> SomethingAsync()
            {
                return Task.FromException<int>(new InvalidOperationException());
            }
        }

        class AsynchronousSuccess : IMyInterface
        {
            public async Task<int> SomethingAsync()
            {
                await Task.Yield(); // force asynchronous behavior
                return 13;
            }
        }

        [TestMethod]
        public async Task ConcreteMockImplementations()
        {
            SynchronousSuccess synchronousSuccess = new();
            SynchronousError synchronousError = new();
            AsynchronousSuccess asynchronousSuccess = new();

            Assert.AreEqual(13, await synchronousSuccess.SomethingAsync());
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => synchronousError.SomethingAsync());
            Assert.AreEqual(13, await asynchronousSuccess.SomethingAsync());
        }
    }
}


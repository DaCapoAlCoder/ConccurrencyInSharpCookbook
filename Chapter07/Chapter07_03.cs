using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter07
{
    [TestClass]
    public class Chapter07_03
    {

        [TestMethod]
        public async Task MyMethodAsync_DoesNotThrow()
        {
            var objectUnderTest = new Sut(); // ...;
            // Cannot await an async void method. This will not work
            //await objectUnderTest.MyVoidMethodAsync();

            // This will not allow the code inside to be awaited
            // It will execute asynchronously but the code will just continue
            // because it cannot be awaited
            objectUnderTest.MyVoidMethodAsync();
            Trace.WriteLine("In test");
        }

        [TestMethod]
        public void MyMethodAsync_DoesNotThrowWithAsyncContext()
        {
            // This async context allows for the async code to executed and waits
            // for all async code to complete. 

            // Its better to avoid async void entirely and just use it to call 
            // an async Task which contains all of the logic. Only use async void
            // if required by something else

            AsyncContext.Run(() =>
            {
                var objectUnderTest = new Sut(); // ...;
                objectUnderTest.MyVoidMethodAsync();
                Trace.WriteLine("In test");
            });
        }



        class Sut
        {
            public async void MyVoidMethodAsync()
            {
                await Task.Delay(2000);
                Trace.WriteLine("In the method");
            }

        }
    }
}

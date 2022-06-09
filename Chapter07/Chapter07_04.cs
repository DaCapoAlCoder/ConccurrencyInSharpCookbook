using Chapter05;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Chapter07
{
    [TestClass]
    public class Chapter07_04
    {

        // If the data mesh is part of a larger component it might be
        // more straight forward to test the component and implicitly test the
        // mesh as a result. If testing a reusable block then testing the block
        // directly should be done

        [TestMethod]
        public async Task MyCustomBlock_AddsOneToDataItems()
        {
            // Create the block, post then receive each result in order
            // to assert on
            Chapter05_06 chapter05_06 = new();
            var myCustomBlock = chapter05_06.CreateMyCustomBlock();

            myCustomBlock.Post(3);
            myCustomBlock.Post(13);
            myCustomBlock.Complete();

            Assert.AreEqual(4, myCustomBlock.Receive());
            Assert.AreEqual(14, myCustomBlock.Receive());
            await myCustomBlock.Completion;
        }

        [TestMethod]
        public async Task MyCustomBlock_Fault_DiscardsDataAndFaults()
        {
            Chapter05_06 chapter05_06 = new();
            var myCustomBlock = chapter05_06.CreateMyCustomBlock();

            myCustomBlock.Post(3);
            myCustomBlock.Post(13);
            // Causes the IDataflowBlock to complete in a TaskStatus.Faulted state
            myCustomBlock.Fault(new InvalidOperationException());

            // Because the data flow mesh will wrap exceptions in an aggregate exception
            // its not enough to use an assert on the exception, it needs to be flattened
            // for the inner exception type to be extracted. So the try catch is required
            try
            {
                await myCustomBlock.Completion;
            }
            catch (AggregateException ex)
            {
                AssertExceptionIs<InvalidOperationException>(
                    ex.Flatten().InnerException, false);
            }
        }

        public static void AssertExceptionIs<TException>(Exception ex,
            bool allowDerivedTypes = true)
        {
            if (allowDerivedTypes && !(ex is TException))
            {
                Assert.Fail($"Exception is of type {ex.GetType().Name}, but " +
                    $"{typeof(TException).Name} or a derived type was expected.");
            }
            if (!allowDerivedTypes && ex.GetType() != typeof(TException))
            {
                Assert.Fail($"Exception is of type {ex.GetType().Name}, but " +
                    $"{typeof(TException).Name} was expected.");
            }
        }
    }
}

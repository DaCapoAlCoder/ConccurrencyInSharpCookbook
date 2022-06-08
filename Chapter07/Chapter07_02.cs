using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter07
{
    class Divider
    {
        public Task<double> Divide(int dividend, int divisor)
        {
            double value = dividend / divisor;
            return Task.FromResult(value);

        }
    }

    [TestClass]
    public class Chapter07_02
    {
        [TestMethod]
        [ExpectedException(typeof(DivideByZeroException))]
        public async Task Divide_WhenDivisorIsZero_ThrowsDivideByZeroExceptionAttribute()
        {
            // This detects an exception anywhere in the unit test code rather than 
            // just the method being tested. Use An Assert Exception instead
            Divider divider = new();
            await divider.Divide(4, 0);
        }

        [TestMethod]
        public async Task Divide_WhenDivisorIsZero_ThrowsDivideByZeroExceptionAssert()
        {
            //This will assert an exception occurs only in the method being tested
            Divider divider = new();
            await Assert.ThrowsExceptionAsync<DivideByZeroException>(async () => await divider.Divide(4, 0));
        }

        /// <summary>
        /// Ensures that an asynchronous delegate throws an exception.
        /// </summary>
        /// <typeparam name="TException">
        /// The type of exeption to expect
        /// </typeparam>
        /// <param name="action">The asynchronous delegate to test.</param>
        /// <param name="allowDerivedTypes">Whether derived types should be accepted</param>
        public static async Task<TException> ThrowsAsync<TException>(Func<Task> action, bool allowDerivedTypes = true) where TException : Exception
        {
            try
            {
                await action();
                var name = typeof(Exception).Name;
                Assert.Fail($"Delegate did not throw expeted exception {name}");
                return null;
            }
            catch (Exception ex)
            {
                if(allowDerivedTypes && !(ex is TException))
                {
                    Assert.Fail($"Delegate threw exception of type {ex.GetType().Name}, " +
                        $"but {typeof(TException).Name} or a derived type was expeced.");

                }

                if(!allowDerivedTypes && ex.GetType() != typeof(TException))
                {
                    Assert.Fail($"Delegate threw exception of type {ex.GetType().Name}, " +
                        $"but {typeof(TException).Name} was expeced.");
                }
                return (TException)ex;
            }
        }

        [TestMethod]
        public async Task Divide_WhenDivisorIsZero_ThrowsDivideByZeroExceptionHandRolled()
        {
            // If the test framework does not have an async exception assert mechanism roll your own
            Divider divider = new();
            await ThrowsAsync<DivideByZeroException>(async () => await divider.Divide(4, 0));
        }
    }
}

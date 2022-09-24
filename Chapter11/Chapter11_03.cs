using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter11
{
    public class Chapter11_03 : IChapterAsync
    {
        public async Task Run()
        {
            // This allows async calls to be made, but nothing happens in code
            // such as exceptions etc., unit the task from the initialisation has
            // is actually awaited. This is a good approach where the initialised
            // value is required outside of the class
            MyFundamentalType myFundamentalType = new();
            var initValue = await myFundamentalType.Initialization;
            Console.WriteLine($"Initialised with value {initValue}");

        }
        interface IMyFundamentalType { }
        interface IMyComposedType { }


        /// <summary>
        /// Marks a type as requiring asynchronous initialization 
        /// and provides the result of that initialization.
        /// </summary>
        public interface IAsyncInitialization
        {
            /// <summary>
            /// The result of the asynchronous initialization of this instance.
            /// </summary>
            Task<int> Initialization { get; }
        }

        class MyFundamentalType : IMyFundamentalType, IAsyncInitialization
        {

            public MyFundamentalType()
            {
                Initialization = InitializeAsync();
                Console.WriteLine("Initialisation started in constructor but await is not called");
            }

            public Task<int> Initialization { get; private set; }

            private async Task<int> InitializeAsync()
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                return await Task.FromResult(22);
            }
        }
    }
}

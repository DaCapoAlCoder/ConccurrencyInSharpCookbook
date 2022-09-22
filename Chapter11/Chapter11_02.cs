using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Chapter11
{
    public class Chapter11_02 : IChapterAsync
    {
        public async Task Run()
        {
            //TODO Read and write up discussion

            // Its not possible to have async constructors and so you can't carry
            // out async initialisation directly in the constructor. One way to  
            // to do it is to use a specific initialisation method. The problem
            // with this approach is that it is easy to forget to use the method
            var instance = new MyAsyncClass();
            await instance.InitializeAsync();

            // A better approach to initialisation is to use the factory pattern
            // Here a static method on the class  is used to create a new instance
            // then call the initialisation method before returning an instance
            var myAsyncClassWithFactory = await MyAsyncClassWithFactory.CreateAsync();
            Console.WriteLine($"Class has been async initialised with value: {myAsyncClassWithFactory.SomeValue}");

            // Async void is used because you can't call a method that would return a Task
            // which would require the constructor being async which is not possible. 
            // Async void does allow an async method being called from the constructor
            // but method should not be used. This approach has the pitfalls of using
            // async void. 
            // The first issue is that the constructor can complete while the async
            // method is still initialising so there's no way to tell when the initialisation
            // has completed. The second issue is with error handling, any exception cannot
            // be caught by a try/catch block surrounding the construction
            MyAsyncClassBadCode myAsyncClassBadCode = new();

            // The problem with the valid approaches here comes where dependency injection
            // is being used. As apparently no dependency injection tool works with async
            // code (I have my doubts about this)
            // From the .Net Docs:
            // Add{LIFETIME}<{ SERVICE}> (sp => new { IMPLEMENTATION })
            // Examples:
            // services.AddSingleton<IMyDep>(sp => new MyDep());
            // Could probably run the async initialisation method after first construction retrain
            // async initialisation and return the initialised object. Could also just inject the
            // factory where an instance is required. A better option might be to use the lazy
            // initialisation to allow the async code to be called only when required rather than
            // on construction
        }

        class MyAsyncClass
        {
            public Task InitializeAsync() 
            { 

                Console.WriteLine($"Class has been async initialised using a public method");
                return Task.CompletedTask; 
            }
        }

        class MyAsyncClassWithFactory
        {
            private int _someValue;

            public int SomeValue => _someValue;

            // The private constructor ensures that the initialisation method
            // is executed as an instance can only be created through the
            // CreateAsync method
            private MyAsyncClassWithFactory()
            {
            }

            private async Task<MyAsyncClassWithFactory> InitializeAsync()
            {
                // Initialise the class and return the current instance
                //await Task.Delay(TimeSpan.FromSeconds(1));
                HttpClient httpClient = new();
                var bytes = await httpClient.GetByteArrayAsync("https://example.com");
                _someValue = bytes.Length;
                return this;
            }

            public static Task<MyAsyncClassWithFactory> CreateAsync()
            {
                // Can create a new instance here because the static
                // method is within the class allowing access to the
                // private constructor
                var result = new MyAsyncClassWithFactory();
                return result.InitializeAsync();
            }
        }
        class MyAsyncClassBadCode
        {
            public MyAsyncClassBadCode()
            {
                InitializeAsync();
            }

            // BAD CODE!!
            private async void InitializeAsync()
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}

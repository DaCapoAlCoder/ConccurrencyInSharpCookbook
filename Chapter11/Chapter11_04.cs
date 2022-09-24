using Common;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter11
{
    public class Chapter11_04 : IChapterAsync
    {
        public async Task Run()
        {
            // This chapter focuses on properties that need to use the async keyword. This
            // happens when making code asynchronous and a property that called a synchronous
            // method now calls an asynchronous method. Properties should return current values
            // not kick off background processes and so properties can't use the async
            // keyword. Here are the ways to work around that. 

            // If the property needs to kick off a new process to get data on every read,
            // use a method instead of the property.
            var data = await GetDataAsync();
            Console.WriteLine($"Getting the data as a method instead of a property {data}");

            // Instead of a method use a property that returns the Task<T> of the type 
            // the synchronous equivalent property would return. The caller can await
            // the task instead of the property. The property is backed by a private
            // method that carries out the async work. This is not recommended as the
            // property is kicking off new background processes on reads and is really
            // a method.
            data = await Data;
            Console.WriteLine($"Not recommended, getting the data from a property returning Task<T>: {data}");

            // AsyncLazy is from the Nito.Async library. It will kick of the async process once and cache it
            // allowing the value to be accessed multiple times without calling the async process each time
            data = await DataAsyncLazy;
            Console.WriteLine($"Got the data the first time from async lazy {data}");

            data = await DataAsyncLazy;
            Console.WriteLine($"Got the data the second time from async lazy: {data}.");
            Console.WriteLine($"The async method was not called the second time");

            // Don't do this, its blocking the current thread using an async method backed property
            data = DataBadImplementation;
            Console.WriteLine($"This data was got by blocking the current thread don't do that {data}");

            // The book goes on to describe how state exposed by properties of an async API should be
            // considered. Stream.Position exposes the position of a stream. But when should it be updated
            // in terms of Stream.ReadAsync and Stream.WriteAsync. There is no clear answer but it should
            // be considered when creating the API
        }

        // As an asynchronous method.
        public async Task<int> GetDataAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            return 13;
        }

        // As a Task-returning property.
        // This API design is questionable.
        public Task<int> Data
        {
            get { return PrivateGetDataAsync(); }
        }

        private async Task<int> PrivateGetDataAsync()
        {
            return await GetDataAsync();
        }

        // As a cached value.
        public AsyncLazy<int> DataAsyncLazy
        {
            get { return _data; }
        }


        private readonly AsyncLazy<int> _data =
            new AsyncLazy<int>(async () =>
            {
                Console.WriteLine("Calling the async method to get the data for AsyncLazy");
                await Task.Delay(TimeSpan.FromSeconds(1));
                return 13;
            });

        public int DataBadImplementation
        {
            // BAD CODE!!
            get { return GetDataAsync().Result; }
        }
    }
}

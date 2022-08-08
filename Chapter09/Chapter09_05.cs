using Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter09
{
    public class Chapter09_05 : IChapter
    {
        public void Run()
        {
            Test();
        }
        void Test()
        {
            var dictionary = new ConcurrentDictionary<int, string>();

            // Delegates should be simple and not have any side effects because
            // the delegates can be called in multiple threads
            string newValue = dictionary.AddOrUpdate(0,
                // This is called if the key doesn't exist
                key => "Zero",
                // This is called if the key already exists
                (key, oldValue) => "Zero");

            // Its possible for one or both of the delegates to be
            // called multiple times as different threads try to 
            // update the dictionary. This is due to a number of while
            // loops running internally as they try to add or update the
            // value successfully. Contention from other threads may cause
            // the loops to execute more than once

            // AddOrUpdate is thread-safe meaning the values won't be
            // corrupted, but they are not atomic, this applies to any
            // concurrent operation using delegates. This means that
            // the delegate from one thread can be executed while the
            // delegate from another is also executed concurrently.
            // The delegates execute outside the lock.
            // The delegates provides the value for the internal add/update.
            // These add/update operations should be protected by some form of locking. 

            // A sequence from GetOrAdd outlines the pitfall:

            // threadA calls GetOrAdd, finds no item, and creates a new item to add by invoking the valueFactory delegate.
            // threadB calls GetOrAdd concurrently, its valueFactory delegate is invoked and it arrives at the internal lock before threadA, and so its new key-value pair is added to the dictionary.
            // threadA's user delegate completes, and the thread arrives at the lock, but now sees that the item exists already.
            // threadA performs a "Get" and returns the data that was previously added by threadB.

            Console.WriteLine($"New value is: {newValue}");


            // Using the same "dictionary" as above.
            // Adds (or updates) key 0 to have the value "Zero".
            // This operation is simpler and will be atomic, but it doesn't
            // provide the ability to update based on the current value as with
            // the explicit AddOrUpdate method
            dictionary[0] = "Zero";


            // Using the same "dictionary" as above.
            bool keyExists = dictionary.TryGetValue(0, out string currentValue);
            Console.WriteLine($"Try get value. Key exists: {keyExists}. Current Value: {currentValue}");

            // Using the same "dictionary" as above.
            bool keyExisted = dictionary.TryRemove(0, out string removedValue);
            Console.WriteLine($"Try get value. Key existed: {keyExisted}. Removed value: {removedValue}");

            // If one thread always reads an another thread always updates then there are more appropriate 
            // producer consumer collections that can be used in place of ConcurrentDictionary
        }
    }
}

namespace Chapter3
{
    public class Chapter3_00
    {
        // Async Streams
        // Standard async can only return once in the form of Task<T>
        // If T is a collection such as Task<List<sting>>, the full list must be calculated before returning
        // Asynchronous Streams are similar to an asynchronous version of an enumerable
        // Enumerables allow for iterating through infinite loops using yield return (e.g a loop that calculates prime numbers)
        // IAsyncEnumerable acts in a similar manner except the data source is asynchronous
        // If IEnumerable is used when using a I/O bound source it will block the calling thread
        // Hence the requirement for the async equivalent
        // Using Task<IEnumerable<T>> would mean all items must be loaded into memory when awaiting
        // this is problematic for infinite loop type situations and the values cannot be calculated as they are required

    }
}

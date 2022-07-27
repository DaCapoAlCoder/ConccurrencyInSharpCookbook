using Common;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter09
{
    public class Chapter09_02 : IChapter
    {
        public void Run()
        {
            // Immutable lists are a general purpose data structure
            // Its internal structure uses a binary tree to share memory
            // This means most operations run at O(LogN) or more (except
            // foreach enumeration at O(N)). Use this type if none of the
            // the others will do the job. It can't just be a straight
            // swap for mutable equivalent without first considering
            // the performance impact. The best use case seems to be
            // where indexing is required along with snapshot processing
            // The book says its rare to use this collection in place of
            // the other immutable collections

            var list = ImmutableList();
            IterateOverImmutableList(list);
        }

       ImmutableList<int> ImmutableList()
        {
            // The immutable list has similar methods to a mutable list

            ImmutableList<int> list = ImmutableList<int>.Empty;
            list = list.Insert(0, 13);
            list = list.Insert(0, 7);

            // Displays "7" followed by "13".
            foreach (int item in list)
            {
                Console.WriteLine(item);
            }

            list = list.RemoveAt(1);
            return list;
        }

        void IterateOverImmutableList(ImmutableList<int> list)
        {
            
            // The best way to iterate over an ImmutableList<T>
            // This operates at O(N) execution time. 
            foreach (var item in list)
            {
                Console.WriteLine(item);
            }

            // This will also work, but it will be much slower.
            // This operates at O(NLog(N)) due to the internal
            // structure. 
            for (int i = 0; i != list.Count; ++i)
            {
                Console.WriteLine(list[i]);
            }
        }
    }
}

using System;
using EDA.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EDA
{
    [TestClass]
    public class UnitTest_MinPQ
    {
        [TestMethod]
        public void TestAddDelMin()
        {
            MinPQ<int> pq = new MinPQ<int>((a, b) => a.CompareTo(b));
            pq.Add(10);
            pq.Add(8);
            pq.Add(6);
            pq.Add(9);
            pq.Add(7);
            pq.Add(5);
            pq.Add(4);
            pq.Add(1);
            pq.Add(3);
            pq.Add(2);
            
            Assert.AreEqual(1, pq.Peek());
            Assert.AreEqual(10, pq.Count);
            for(int i=1; i < 10; ++i)
            {
                Assert.AreEqual(10 - i + 1, pq.Count);
                int removed = pq.DelMin();
                
                Console.WriteLine("Remove {0}", removed);
                Console.WriteLine("Count: {0}", pq.Count);
                PrintArray(pq.ToArray());

                Assert.AreEqual(i, removed);
                
            }
        }

        private void PrintArray(int[] arr)
        {
            foreach(var i in arr)
            {
                Console.Write(i + " ");
            }
            Console.WriteLine();
        }
    }
}

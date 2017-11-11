using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDA.Helpers
{
    public class MinPQ<T>
    {
        private Func<T, T, int> Compare;
        private T[] s;
        private int N;

        public MinPQ(Func<T, T, int> compare, int capacity = 10)
        {
            s = new T[capacity+1];
            N = 0;
            Compare = compare;
        }

        public int Count { get { return N;  } }
        public bool IsEmpty {  get { return N == 0; } }
        public void Add(T item)
        {
            if(N+1 >= s.Length)
            {
                Resize(s.Length * 2);
            }
            s[++N] = item;
            Swim(N);
        }

        public T Peek()
        {
            if(N == 0)
            {
                return default(T);
            }

            return s[1];
        }

        public T DelMin()
        {
            if(N == 0)
            {
                throw new IndexOutOfRangeException();
            }
            T item = s[1];
            Swap(s, 1, N--);
            s[N + 1] = default(T);
            if(N <= s.Length / 4)
            {
                Resize(s.Length / 2);
            }

            Sink(1);
            return item;
        }

        private bool Less(T a, T b)
        {
            return Compare(a, b) < 0;
        }

        private static void Swap(T[] a, int i, int j)
        {
            T temp = a[i];
            a[i] = a[j];
            a[j] = temp;
        }

        private void Swim(int k)
        {
            while(k > 1)
            {
                int parent = k / 2;
                if(Less(s[k], s[parent]))
                {
                    Swap(s, k, parent);
                    k = parent;
                } else
                {
                    break;
                }
            }
        }

        private void Sink(int k)
        {
            while(k * 2 <= N)
            {
                int child = k * 2;
                if(child < N && Less(s[child+1], s[child]))
                {
                    child++;
                }
                if(Less(s[child], s[k]))
                {
                    Swap(s, child, k);
                    k = child;
                } else
                {
                    break;
                }
            }
        }

        private void Resize(int newSize)
        {
            T[] temp = new T[newSize];
            int len = Math.Min(s.Length, newSize);
            for(int i=0; i <  len; ++i)
            {
                temp[i] = s[i];
            }
            s = temp;
        }

        public T[] ToArray()
        {
            T[] result = new T[N];
            for(int i=0; i < N; ++i)
            {
                result[i] = s[i+1];
            }
            return result;
        }
    }
}

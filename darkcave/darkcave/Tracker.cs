using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace darkcave
{
    public class Tracker<T>
    {
        private T[] track;
        private int pointer;
        private int count;

        public Tracker(int length)
        {
            track = new T[length];
            pointer = 0;
            count = length;
        }

        public T Push(T obj)
        {
            track[pointer++] = obj;
            if (pointer == count)
                pointer = 0;
            return obj;
        }

        public T Pop()
        {
            T obj = track[pointer--];
            if (pointer == -1)
                pointer = count - 1;
            return obj;
        }

        public void Next()
        {
            pointer++;
        }

        public T this[int index]
        {
            get
            {
                int indextoRet = (pointer + index + count - 1) % count;
                return track[indextoRet];
            }
        }
    }
}

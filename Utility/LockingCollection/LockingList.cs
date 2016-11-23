using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boredbone.Utility.LockingCollection
{
    public class LockingList<T>
    {
        protected object gate = new object();

        protected List<T> list;

        public LockingList()
        {
            this.list = new List<T>();
        }

        public void Add(T item)
        {
            lock (this.gate)
            {
                this.list.Add(item);
            }
        }

        public void ForEach(Action<T> action)
        {
            lock (this.gate)
            {
                foreach (var item in this.list)
                {
                    action(item);
                }
            }
        }

        public T[] ToArray()
        {
            lock (this.gate)
            {
                return this.list.ToArray();
            }
        }

        public void Clear()
        {
            lock (this.gate)
            {
                this.list.Clear();
            }
        }
    }
}

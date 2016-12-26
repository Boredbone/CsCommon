using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boredbone.Utility.LockingCollection
{
    public class LockingDictionary<TKey, TValue>
    {
        protected object gate = new object();

        protected Dictionary<TKey, TValue> dictionary;

        public LockingDictionary()
        {
            this.dictionary = new Dictionary<TKey, TValue>();
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            lock (this.gate)
            {
                TValue result;
                if (this.dictionary.TryGetValue(key, out result))
                {
                    var item = updateValueFactory(key, result);
                    this.dictionary[key] = item;
                    return item;
                }
                this.dictionary[key] = addValue;
                return addValue;
            }
        }


        public TValue this[TKey key]
        {
            set
            {
                lock (this.gate)
                { this.dictionary[key] = value; }
            }
            get
            {
                lock (this.gate)
                { return this.dictionary[key]; }
            }
        }

        public void ForEach(Action<KeyValuePair<TKey, TValue>> action)
        {
            lock (this.gate)
            {
                foreach (var item in this.dictionary)
                {
                    action(item);
                }
            }
        }

        public void Clear()
        {
            lock (this.gate)
            {
                this.dictionary.Clear();
            }
        }

    }
}

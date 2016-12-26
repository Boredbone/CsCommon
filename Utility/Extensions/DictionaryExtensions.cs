using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsCommon.Utility.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrAlternative<TKey, TValue>
            (this IDictionary<TKey, TValue> dictionary, TKey key, TValue alternative)
        {
            TValue result;
            if (dictionary.TryGetValue(key, out result))
            {
                return result;
            }
            return alternative;
        }

        public static TValue GetValueOrAlternative<TKey, TValue>
            (this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> alternative)
        {
            TValue result;
            if (dictionary.TryGetValue(key, out result))
            {
                return result;
            }
            return alternative();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW
{
    public static class MethodExtensionsUtility
    {
        public static void AddRange<T>( this IList<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
                list.Add(item);
        }
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> dictionaryToAdd)
        {
            foreach (var keyValuePair in dictionaryToAdd)
                dictionary.Add(keyValuePair);
        }
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable< KeyValuePair<TKey, TValue>> keyValuePairs)
        {
            foreach (var keyValuePair in keyValuePairs)
                dictionary.Add(keyValuePair);
        }
    }
}

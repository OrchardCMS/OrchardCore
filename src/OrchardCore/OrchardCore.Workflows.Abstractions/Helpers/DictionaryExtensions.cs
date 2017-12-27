using System.Collections.Generic;

namespace OrchardCore.Workflows.Helpers
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Safely tries and returns a value by the specified key. If the specified key does not exist, null is returned.
        /// </summary>
        public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : default(TValue);
        }

        /// <summary>
        /// Merges the specified other dictionary into this dictionary.
        /// Existing entries are overwritten.
        /// </summary>
        public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> other)
        {
            foreach (var item in other)
            {
                dictionary[item.Key] = item.Value;
            }
        }
    }
}

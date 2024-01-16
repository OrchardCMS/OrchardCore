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
            return dictionary.TryGetValue(key, out var value) ? value : default;
        }

        /// <summary>
        /// Safely tries and returns a value by the specified key. If the specified key does not exist, null is returned.
        /// </summary>
        public static TValue GetValue<TValue>(this IDictionary<string, object> dictionary, string key)
        {
            var value = dictionary.GetValue(key);

            if (value != null)
            {
                return (TValue)value;
            }

            return default;
        }

        /// <summary>
        /// Merges the specified other dictionary into this dictionary.
        /// Existing entries are overwritten.
        /// </summary>
        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> other)
        {
            var copy = new Dictionary<TKey, TValue>(dictionary);
            foreach (var item in other)
            {
                copy[item.Key] = item.Value;
            }
            return copy;
        }
    }
}

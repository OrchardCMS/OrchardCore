using System.Collections.Generic;

namespace OrchardCore.Workflows.Helpers
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Safely tries and returns a value by the specified key. If the specified key does not exist, null is returned.
        /// </summary>
        public static object GetValue(this IDictionary<string, object> dictionary, string key)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : null;
        }
    }
}

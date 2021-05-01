using System.Collections.Generic;

namespace OrchardCore.AuditTrail.Extensions
{
    public static class CollectionExtensions
    {
        public static void AddToListIfNotNull<T>(this List<T> list, T value) where T : class
        {
            if (value != null)
            {
                list.Add(value);
            }
        }

        public static T Get<T>(this Dictionary<string, object> data, string key)
        {
            if (data != null && data.TryGetValue(key, out var value))
            {
                if (value is T result)
                {
                    return result;
                }
            }

            return default;
        }
    }
}

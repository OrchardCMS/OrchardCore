using System.Collections.Generic;
using System.Linq;
using Cysharp.Text;
using OrchardCore.Environment.Cache;

namespace OrchardCore.DynamicCache
{
    public static class CacheContextEntryExtensions
    {
        public static string GetContextHash(this IEnumerable<CacheContextEntry> entries)
        {
            using var sb = ZString.CreateStringBuilder();
            foreach (var entry in entries.OrderBy(x => x.Key).ThenBy(x => x.Value))
            {
                var part = entry.Key + entry.Value;
                sb.Append(part);
            }

            return sb.ToString();
        }
    }
}

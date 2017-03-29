using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Environment.Cache;

namespace Orchard.DynamicCache
{
    public static class CacheContextManagerExtensions
    {
        public static string GetContextHash(this IEnumerable<CacheContextEntry> entries)
        {
            var sb = new StringBuilder();
            foreach (var entry in entries.OrderBy(x => x.Key).ThenBy(x => x.Value))
            {
                var part = entry.Key + entry.Value;
                sb.Append(part);
            }

            return sb.ToString();
        }
    }
}

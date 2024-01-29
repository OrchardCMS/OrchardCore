using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.DynamicCache.TagHelpers
{
    public class DynamicCacheTagHelperService
    {
        public ConcurrentDictionary<string, Task<IHtmlContent>> Workers = new();
    }
}

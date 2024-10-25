using System.Collections.Concurrent;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.DynamicCache.TagHelpers;

public class DynamicCacheTagHelperService
{
    public ConcurrentDictionary<string, Task<IHtmlContent>> Workers = new();
}

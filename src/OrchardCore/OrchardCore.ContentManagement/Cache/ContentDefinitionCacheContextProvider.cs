using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Environment.Cache;

namespace OrchardCore.ContentManagement.Cache
{
    public class ContentDefinitionCacheContextProvider : ICacheContextProvider
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentDefinitionCacheContextProvider(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public async Task PopulateContextEntriesAsync(IEnumerable<string> contexts, List<CacheContextEntry> entries)
        {
            if (contexts.Any(ctx => String.Equals(ctx, "types", StringComparison.OrdinalIgnoreCase)))
            {
                var hash = await _contentDefinitionManager.GetTypesHashAsync();

                // Add a hash based on the content definition record serial number.
                entries.Add(new CacheContextEntry("types", hash.ToString(CultureInfo.InvariantCulture)));

                return;
            }
        }
    }
}

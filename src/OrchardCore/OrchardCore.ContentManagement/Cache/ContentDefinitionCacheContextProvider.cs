using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Environment.Cache;

namespace OrchardCore.ContentManagement.Cache;

public class ContentDefinitionCacheContextProvider : ICacheContextProvider
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public ContentDefinitionCacheContextProvider(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task PopulateContextEntriesAsync(IEnumerable<string> contexts, List<CacheContextEntry> entries)
    {
        if (contexts.ContainsContext("types"))
        {
            var identifier = await _contentDefinitionManager.GetIdentifierAsync();
            entries.Add(new CacheContextEntry("types", identifier));

            return;
        }
    }
}

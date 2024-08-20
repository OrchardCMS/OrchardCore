using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Handlers;

public class AzureAISearchFieldIndexEventsBase : IAzureAISearchFieldIndexEvents
{
    public virtual Task MappingAsync(SearchIndexDefinition context)
        => Task.CompletedTask;

    public virtual Task MappedAsync(SearchIndexDefinition context)
        => Task.CompletedTask;
}

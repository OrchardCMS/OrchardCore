using OrchardCore.AzureAI.Models;

namespace OrchardCore.AzureAI;

public interface IAzureAISearchFieldIndexEvents
{
    Task MappingAsync(SearchIndexDefinition context);

    Task MappedAsync(SearchIndexDefinition context);
}

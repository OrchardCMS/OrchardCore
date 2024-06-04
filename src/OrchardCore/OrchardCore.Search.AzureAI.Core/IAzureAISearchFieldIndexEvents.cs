using System.Threading.Tasks;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI;

public interface IAzureAISearchFieldIndexEvents
{
    Task MappingAsync(SearchIndexDefinition context);

    Task MappedAsync(SearchIndexDefinition context);
}

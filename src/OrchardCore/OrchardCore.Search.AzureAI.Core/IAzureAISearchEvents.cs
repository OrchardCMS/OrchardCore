using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI;

public interface IAzureAISearchEvents
{
    Task MappingAsync(AzureAISearchMappingContext context);
}

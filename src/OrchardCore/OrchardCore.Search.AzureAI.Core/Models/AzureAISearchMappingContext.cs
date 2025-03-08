namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchMappingContext
{
    public readonly AzureAISearchIndexSettings Settings;

    public AzureAISearchMappingContext(AzureAISearchIndexSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        Settings = settings;
    }
}

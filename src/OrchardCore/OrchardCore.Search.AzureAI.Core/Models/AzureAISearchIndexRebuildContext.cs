namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchIndexRebuildContext
{
    public AzureAISearchIndexSettings Settings { get; }

    public AzureAISearchIndexRebuildContext(AzureAISearchIndexSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings = settings;
    }
}

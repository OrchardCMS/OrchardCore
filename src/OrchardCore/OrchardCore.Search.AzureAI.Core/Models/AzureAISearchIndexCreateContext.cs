namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchIndexCreateContext
{
    public AzureAISearchIndexSettings Settings { get; }

    public AzureAISearchIndexCreateContext(AzureAISearchIndexSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings = settings;
    }
}

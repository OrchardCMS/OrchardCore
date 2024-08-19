namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchIndexCreateContext
{
    public AzureAISearchIndexSettings Settings { get; }

    public string IndexFullName { get; }

    public AzureAISearchIndexCreateContext(AzureAISearchIndexSettings settings, string indexFullName)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings = settings;
        IndexFullName = indexFullName;
    }
}

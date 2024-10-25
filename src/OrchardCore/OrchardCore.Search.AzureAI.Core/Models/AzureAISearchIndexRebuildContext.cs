namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchIndexRebuildContext
{
    public AzureAISearchIndexSettings Settings { get; }

    public string IndexFullName { get; }

    public AzureAISearchIndexRebuildContext(AzureAISearchIndexSettings settings, string indexFullName)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings = settings;
        IndexFullName = indexFullName;
    }
}

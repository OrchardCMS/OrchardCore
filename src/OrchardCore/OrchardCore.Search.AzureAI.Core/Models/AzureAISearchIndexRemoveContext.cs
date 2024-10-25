namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchIndexRemoveContext(string indexName, string indexFullName)
{
    public string IndexName { get; } = indexName;

    public string IndexFullName { get; } = indexFullName;
}

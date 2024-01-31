namespace OrchardCore.Search.AzureAI;

public class AzureAISearchIndexRemoveContext(string indexName, string indexFullName)
{
    public string IndexName { get; } = indexName;

    public string IndexFullName { get; } = indexFullName;
}

namespace OrchardCore.Search.AzureAI.Models;

public sealed class AzureAISearchDefaultQueryMetadata
{
    public string QueryAnalyzerName { get; set; }

    public string[] DefaultSearchFields { get; set; }
}

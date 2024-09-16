using OrchardCore.Data.Documents;

namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchIndexSettingsDocument : Document
{
    public Dictionary<string, AzureAISearchIndexSettings> IndexSettings { get; set; } = [];
}

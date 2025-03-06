using OrchardCore.DisplayManagement;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.ViewModels;

public class AzureAIIndexEntry
{
    public AzureAISearchIndexSettings Index { get; set; }

    public IShape Shape { get; set; }
}

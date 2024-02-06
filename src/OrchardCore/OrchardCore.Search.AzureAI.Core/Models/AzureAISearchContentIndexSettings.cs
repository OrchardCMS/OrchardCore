using OrchardCore.Indexing;

namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchContentIndexSettings : IContentIndexSettings
{
    public bool Included { get; set; }

    public DocumentIndexOptions ToOptions()
    {
        var options = DocumentIndexOptions.None;

        return options;
    }
}

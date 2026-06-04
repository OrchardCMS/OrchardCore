using OrchardCore.Indexing;

namespace OrchardCore.AzureAI.Models;

public class AzureAISearchContentIndexSettings : IContentIndexSettings
{
    public bool Included { get; set; }

    public DocumentIndexOptions ToOptions()
    {
        var options = DocumentIndexOptions.None;

        return options;
    }
}

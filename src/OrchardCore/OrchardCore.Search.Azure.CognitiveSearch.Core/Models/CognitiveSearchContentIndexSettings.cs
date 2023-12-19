using OrchardCore.Indexing;

namespace OrchardCore.Search.Azure.CognitiveSearch.Models;

public class CognitiveSearchContentIndexSettings : IContentIndexSettings
{
    public bool Included { get; set; }

    public DocumentIndexOptions ToOptions()
    {
        var options = DocumentIndexOptions.None;

        return options;
    }
}

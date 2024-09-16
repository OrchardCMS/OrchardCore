using OrchardCore.Indexing;
using static OrchardCore.Indexing.DocumentIndexBase;

namespace OrchardCore.Search.AzureAI.Models;

public class AzureAISearchIndexMap
{
    public string IndexingKey { get; set; }

    public string AzureFieldKey { get; set; }

    public Types Type { get; set; }

    public bool IsKey { get; set; }

    public bool IsCollection { get; set; }

    public bool IsSuggester { get; set; }

    public bool IsFilterable { get; set; }

    public bool IsSortable { get; set; }

    public bool IsHidden { get; set; }

    public bool IsFacetable { get; set; }

    public bool IsSearchable { get; set; }

    public DocumentIndexOptions Options { get; set; }

    public AzureAISearchIndexMap()
    {

    }

    public AzureAISearchIndexMap(string azureFieldKey, Types type)
    {
        ArgumentException.ThrowIfNullOrEmpty(azureFieldKey);

        AzureFieldKey = azureFieldKey;
        Type = type;
    }

    public AzureAISearchIndexMap(string azureFieldKey, Types type, DocumentIndexOptions options)
        : this(azureFieldKey, type)
    {
        Options = options;
    }
}

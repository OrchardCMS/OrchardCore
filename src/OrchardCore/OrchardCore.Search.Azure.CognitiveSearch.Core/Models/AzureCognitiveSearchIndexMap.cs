using System;
using OrchardCore.Indexing;
using static OrchardCore.Indexing.DocumentIndex;

namespace OrchardCore.Search.Azure.CognitiveSearch.Models;

public class AzureCognitiveSearchIndexMap
{
    public string Key { get; set; }

    public Types Type { get; set; }

    public DocumentIndexOptions Options { get; set; }

    public AzureCognitiveSearchIndexMap()
    {

    }

    public AzureCognitiveSearchIndexMap(string key, Types type)
    {
        ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));

        Key = key;
        Type = type;
    }

    public AzureCognitiveSearchIndexMap(string key, Types type, DocumentIndexOptions options)
        : this(key, type)
    {
        Options = options;
    }
}

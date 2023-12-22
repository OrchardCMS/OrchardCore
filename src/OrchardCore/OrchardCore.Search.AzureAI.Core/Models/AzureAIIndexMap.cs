using System;
using OrchardCore.Indexing;
using static OrchardCore.Indexing.DocumentIndex;

namespace OrchardCore.Search.AzureAI.Models;

public class AzureAIIndexMap
{
    public string Key { get; set; }

    public Types Type { get; set; }

    public DocumentIndexOptions Options { get; set; }

    public AzureAIIndexMap()
    {

    }

    public AzureAIIndexMap(string key, Types type)
    {
        ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));

        Key = key;
        Type = type;
    }

    public AzureAIIndexMap(string key, Types type, DocumentIndexOptions options)
        : this(key, type)
    {
        Options = options;
    }
}

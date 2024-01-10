using System;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI;

public class AzureAISearchIndexRebuildContext
{
    public AzureAISearchIndexSettings Settings { get; }

    public string IndexFullName { get; }

    public AzureAISearchIndexRebuildContext(AzureAISearchIndexSettings settings, string indexFullName)
    {
        ArgumentNullException.ThrowIfNull(nameof(settings));

        Settings = settings;
        IndexFullName = indexFullName;
    }
}

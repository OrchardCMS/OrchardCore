using System;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI;

public class AzureAISearchIndexCreateContext
{
    public AzureAISearchIndexSettings Settings { get; }

    public string IndexFullName { get; }

    public AzureAISearchIndexCreateContext(AzureAISearchIndexSettings settings, string indexFullName)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings = settings;
        IndexFullName = indexFullName;
    }
}

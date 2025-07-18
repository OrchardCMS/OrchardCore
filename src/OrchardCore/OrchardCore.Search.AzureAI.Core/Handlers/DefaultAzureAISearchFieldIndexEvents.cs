using OrchardCore.Contents.Indexing;
using OrchardCore.Indexing;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;

namespace OrchardCore.Search.AzureAI.Handlers;

public sealed class DefaultAzureAISearchFieldIndexEvents : IAzureAISearchFieldIndexEvents
{
    public Task MappingAsync(SearchIndexDefinition context)
    {
        if (context.IndexEntry.Type == DocumentIndex.Types.Text)
        {
            context.Map.IsCollection = !context.IndexEntry.Options.HasFlag(DocumentIndexOptions.Keyword);
            context.Map.IsSearchable = true;
        }
        else
        {
            context.Map.IsFilterable = context.IsRoolField;
            context.Map.IsSortable = context.IsRoolField;
        }

        return Task.CompletedTask;
    }

    public Task MappedAsync(SearchIndexDefinition context)
    {
        if (context.Map.AzureFieldKey == AzureAISearchIndexManager.FullTextKey)
        {
            context.Map.IsSearchable = true;
            context.Map.IsCollection = false;
            context.Map.IsSuggester = context.IsRoolField;
        }
        else if (context.Map.AzureFieldKey == AzureAISearchIndexManager.DisplayTextAnalyzedKey)
        {
            context.Map.IsSearchable = true;
            context.Map.IsCollection = false;
        }
        else if (context.Map.AzureFieldKey == ContentIndexingConstants.ContentItemIdKey)
        {
            context.Map.IsKey = context.IsRoolField; // Only the root field should be marked as a key.
            context.Map.IsFilterable = context.IsRoolField;
            context.Map.IsSortable = context.IsRoolField;
        }
        else if (context.Map.AzureFieldKey == ContentIndexingConstants.ContentItemVersionIdKey ||
            context.Map.AzureFieldKey == ContentIndexingConstants.OwnerKey)
        {
            context.Map.IsFilterable = context.IsRoolField;
            context.Map.IsSortable = context.IsRoolField;
        }

        return Task.CompletedTask;
    }
}

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
            context.Map.IsFilterable = true;
            context.Map.IsSortable = true;
        }

        return Task.CompletedTask;
    }

    public Task MappedAsync(SearchIndexDefinition context)
    {
        if (context.Map.AzureFieldKey == AzureAISearchIndexManager.FullTextKey)
        {
            context.Map.IsSearchable = true;
            context.Map.IsCollection = false;
            context.Map.IsSuggester = true;
        }
        else if (context.Map.AzureFieldKey == AzureAISearchIndexManager.DisplayTextAnalyzedKey)
        {
            context.Map.IsSearchable = true;
            context.Map.IsCollection = false;
        }
        else if (context.Map.AzureFieldKey == ContentIndexingConstants.ContentItemIdKey)
        {
            context.Map.IsKey = true;
            context.Map.IsFilterable = true;
            context.Map.IsSortable = true;
        }
        else if (context.Map.AzureFieldKey == ContentIndexingConstants.ContentItemVersionIdKey ||
            context.Map.AzureFieldKey == ContentIndexingConstants.OwnerKey)
        {
            context.Map.IsFilterable = true;
            context.Map.IsSortable = true;
        }

        return Task.CompletedTask;
    }
}

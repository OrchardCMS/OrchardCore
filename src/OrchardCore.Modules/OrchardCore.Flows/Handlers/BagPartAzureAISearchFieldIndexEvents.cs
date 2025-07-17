using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Flows.Models;
using OrchardCore.Indexing;
using OrchardCore.Search.AzureAI.Handlers;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Flows.Handlers;

internal sealed class BagPartAzureAISearchFieldIndexEvents : AzureAISearchFieldIndexEventsBase
{
    public override Task MappingAsync(SearchIndexDefinition context)
    {
        if (context.IndexEntry.Type == DocumentIndex.Types.Complex &&
            context.IndexEntry.Metadata?.TryGetValue(nameof(ContentTypePartDefinition), out var definition) == true &&
            definition is ContentTypePartDefinition contentTypePartDefinition && contentTypePartDefinition.PartDefinition.Name == nameof(BagPart))
        {
            context.Map.SubFields ??= new List<AzureAISearchIndexMap>();

            var contentItemsField = context.Map.SubFields.FirstOrDefault(x => x.AzureFieldKey == "ContentItems");

            if (contentItemsField is null)
            {
                contentItemsField = new AzureAISearchIndexMap("ContentItems", DocumentIndex.Types.Complex)
                {
                    IsCollection = true,
                    SubFields = new List<AzureAISearchIndexMap>(),
                };

                context.Map.SubFields.Add(contentItemsField);
            }

            if (!contentItemsField.SubFields.Any(x => x.AzureFieldKey == "ContentItemId"))
            {
                contentItemsField.SubFields.Add(new AzureAISearchIndexMap("ContentItemId", DocumentIndex.Types.Text, DocumentIndexOptions.Keyword));
            }

            if (!contentItemsField.SubFields.Any(x => x.AzureFieldKey == "ContentItemVersionId"))
            {
                contentItemsField.SubFields.Add(new AzureAISearchIndexMap("ContentItemVersionId", DocumentIndex.Types.Text, DocumentIndexOptions.Keyword));
            }

            if (!contentItemsField.SubFields.Any(x => x.AzureFieldKey == "DisplayText"))
            {
                contentItemsField.SubFields.Add(new AzureAISearchIndexMap("DisplayText", DocumentIndex.Types.Text)
                {
                    IsSearchable = true,
                });
            }

            if (!contentItemsField.SubFields.Any(x => x.AzureFieldKey == "Latest"))
            {
                contentItemsField.SubFields.Add(new AzureAISearchIndexMap("Latest", DocumentIndex.Types.Boolean));
            }

            if (!contentItemsField.SubFields.Any(x => x.AzureFieldKey == "Published"))
            {
                contentItemsField.SubFields.Add(new AzureAISearchIndexMap("Published", DocumentIndex.Types.Boolean));
            }

            if (!contentItemsField.SubFields.Any(x => x.AzureFieldKey == "ModifiedUtc"))
            {
                contentItemsField.SubFields.Add(new AzureAISearchIndexMap("ModifiedUtc", DocumentIndex.Types.DateTime));
            }

            if (!contentItemsField.SubFields.Any(x => x.AzureFieldKey == "PublishedUtc"))
            {
                contentItemsField.SubFields.Add(new AzureAISearchIndexMap("PublishedUtc", DocumentIndex.Types.DateTime));
            }

            if (!contentItemsField.SubFields.Any(x => x.AzureFieldKey == "CreatedUtc"))
            {
                contentItemsField.SubFields.Add(new AzureAISearchIndexMap("CreatedUtc", DocumentIndex.Types.DateTime));
            }

            if (!contentItemsField.SubFields.Any(x => x.AzureFieldKey == "Owner"))
            {
                contentItemsField.SubFields.Add(new AzureAISearchIndexMap("Owner", DocumentIndex.Types.Text, DocumentIndexOptions.Keyword));
            }

            if (!contentItemsField.SubFields.Any(x => x.AzureFieldKey == "Author"))
            {
                contentItemsField.SubFields.Add(new AzureAISearchIndexMap("Author", DocumentIndex.Types.Text, DocumentIndexOptions.Keyword));
            }
        }

        return Task.CompletedTask;
    }
}

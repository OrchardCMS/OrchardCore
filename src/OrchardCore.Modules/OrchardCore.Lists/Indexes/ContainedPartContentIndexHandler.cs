using OrchardCore.ContentManagement;
using OrchardCore.Contents.Indexing;
using OrchardCore.Indexing;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.Indexes;

public class ContainedPartContentIndexHandler : IDocumentIndexHandler
{
    public Task BuildIndexAsync(BuildDocumentIndexContext context)
    {
        if (context.Record is not ContentItem contentItem)
        {
            return Task.CompletedTask;
        }

        var parent = contentItem.As<ContainedPart>();

        if (parent == null)
        {
            return Task.CompletedTask;
        }

        context.DocumentIndex.Set(
            ContentIndexingConstants.ContainedPartKey + ContentIndexingConstants.IdsKey,
            parent.ListContentItemId,
            DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

        context.DocumentIndex.Set(
            ContentIndexingConstants.ContainedPartKey + ContentIndexingConstants.OrderKey,
            parent.Order,
            DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

        return Task.CompletedTask;
    }
}

using OrchardCore.ContentManagement;
using OrchardCore.Contents.Indexing;
using OrchardCore.Indexing;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.Indexes;

public class ContainedPartContentIndexHandler : IContentItemIndexHandler
{
    public Task BuildIndexAsync(BuildIndexContext context)
    {
        var parent = context.ContentItem.As<ContainedPart>();

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

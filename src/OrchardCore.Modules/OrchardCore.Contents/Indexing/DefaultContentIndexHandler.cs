using OrchardCore.ContentManagement;
using OrchardCore.Indexing;

namespace OrchardCore.Contents.Indexing;

public class DefaultContentIndexHandler : IDocumentIndexHandler
{
    public Task BuildIndexAsync(BuildDocumentIndexContext context)
    {
        if (context.Record is not ContentItem contentItem)
        {
            return Task.CompletedTask;
        }

        context.DocumentIndex.Set(
            ContentIndexingConstants.ContentTypeKey,
            contentItem.ContentType,
            DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

        context.DocumentIndex.Set(
            ContentIndexingConstants.CreatedUtcKey,
            contentItem.CreatedUtc,
            DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

        context.DocumentIndex.Set(
            ContentIndexingConstants.LatestKey,
            contentItem.Latest,
            DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

        context.DocumentIndex.Set(
            ContentIndexingConstants.OwnerKey,
            contentItem.Owner,
            DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

        context.DocumentIndex.Set(
            ContentIndexingConstants.AuthorKey,
            contentItem.Author,
            DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

        context.DocumentIndex.Set(
            ContentIndexingConstants.ModifiedUtcKey,
            contentItem.ModifiedUtc,
            DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

        // We need to store because of ContentPickerResultProvider(s)
        context.DocumentIndex.Set(
            ContentIndexingConstants.PublishedKey,
            contentItem.Published,
            DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

        context.DocumentIndex.Set(
            ContentIndexingConstants.PublishedUtcKey,
            contentItem.PublishedUtc,
            DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

        return Task.CompletedTask;
    }
}

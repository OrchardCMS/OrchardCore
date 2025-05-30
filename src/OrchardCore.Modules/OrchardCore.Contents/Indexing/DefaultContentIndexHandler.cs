using OrchardCore.Indexing;

namespace OrchardCore.Contents.Indexing;

public class DefaultContentIndexHandler : IContentItemIndexHandler
{
    public Task BuildIndexAsync(BuildIndexContext context)
    {
        context.DocumentIndex.Set(
            ContentIndexingConstants.ContentTypeKey,
            context.ContentItem.ContentType,
            DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

        context.DocumentIndex.Set(
            ContentIndexingConstants.CreatedUtcKey,
            context.ContentItem.CreatedUtc,
            DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

        context.DocumentIndex.Set(
            ContentIndexingConstants.LatestKey,
            context.ContentItem.Latest,
            DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

        context.DocumentIndex.Set(
            ContentIndexingConstants.OwnerKey,
            context.ContentItem.Owner,
            DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

        context.DocumentIndex.Set(
            ContentIndexingConstants.AuthorKey,
            context.ContentItem.Author,
            DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

        context.DocumentIndex.Set(
            ContentIndexingConstants.ModifiedUtcKey,
            context.ContentItem.ModifiedUtc,
            DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

        // We need to store because of ContentPickerResultProvider(s)
        context.DocumentIndex.Set(
            ContentIndexingConstants.PublishedKey,
            context.ContentItem.Published,
            DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

        context.DocumentIndex.Set(
            ContentIndexingConstants.PublishedUtcKey,
            context.ContentItem.PublishedUtc,
            DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

        return Task.CompletedTask;
    }
}

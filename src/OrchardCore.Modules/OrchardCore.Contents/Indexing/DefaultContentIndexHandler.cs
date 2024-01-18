using System.Threading.Tasks;
using OrchardCore.Indexing;

namespace OrchardCore.Contents.Indexing
{
    public class DefaultContentIndexHandler : IContentItemIndexHandler
    {
        public Task BuildIndexAsync(BuildIndexContext context)
        {
            context.DocumentIndex.Set(
                IndexingConstants.ContentTypeKey,
                context.ContentItem.ContentType,
                DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                IndexingConstants.CreatedUtcKey,
                context.ContentItem.CreatedUtc,
                DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                IndexingConstants.LatestKey,
                context.ContentItem.Latest,
                DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                IndexingConstants.OwnerKey,
                context.ContentItem.Owner,
                DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                IndexingConstants.AuthorKey,
                context.ContentItem.Author,
                DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                IndexingConstants.ModifiedUtcKey,
                context.ContentItem.ModifiedUtc,
                DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

            // We need to store because of ContentPickerResultProvider(s)
            context.DocumentIndex.Set(
                IndexingConstants.PublishedKey,
                context.ContentItem.Published,
                DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                IndexingConstants.PublishedUtcKey,
                context.ContentItem.PublishedUtc,
                DocumentIndexOptions.Keyword | DocumentIndexOptions.Store);

            return Task.CompletedTask;
        }
    }
}

using System.Threading.Tasks;
using OrchardCore.Indexing;

namespace OrchardCore.Contents.Indexing
{
    public class DefaultContentIndexHandler : IContentItemIndexHandler
    {
        public Task BuildIndexAsync(BuildIndexContext context)
        {
            // Text values are analyzed but stored in field.keyword naturally
            context.DocumentIndex.Set(
                IndexingConstants.ContentTypeKey,
                context.ContentItem.ContentType,
                DocumentIndexOptions.Analyze);

            context.DocumentIndex.Set(
                IndexingConstants.CreatedUtcKey,
                context.ContentItem.CreatedUtc,
                DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                IndexingConstants.LatestKey,
                context.ContentItem.Latest,
                DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                IndexingConstants.OwnerKey,
                context.ContentItem.Owner,
                DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                IndexingConstants.AuthorKey,
                context.ContentItem.Author,
                DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                IndexingConstants.ModifiedUtcKey,
                context.ContentItem.ModifiedUtc,
                DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                IndexingConstants.PublishedKey,
                context.ContentItem.Published,
                DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                IndexingConstants.PublishedUtcKey,
                context.ContentItem.PublishedUtc,
                DocumentIndexOptions.Store);

            return Task.CompletedTask;
        }
    }
}

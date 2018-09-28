using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Indexing;

namespace OrchardCore.Contents.Indexing
{
    public class DefaultContentIndexHandler : IContentItemIndexHandler
    {
        public Task BuildIndexAsync(BuildIndexContext context, ContentItem contentItem)
        {
            context.DocumentIndex.Set(
                IndexingConstants.ContentItemIdKey,
                contentItem.ContentItemId,
                DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                IndexingConstants.ContentItemVersionIdKey,
                contentItem.ContentItemVersionId,
                DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                IndexingConstants.ContentTypeKey,
                contentItem.ContentType,
                DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                IndexingConstants.CreatedUtcKey,
                contentItem.CreatedUtc,
                DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                IndexingConstants.LatestKey,
                contentItem.Latest,
                DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                IndexingConstants.OwnerKey,
                contentItem.Owner,
                DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                IndexingConstants.AuthorKey,
                contentItem.Author,
                DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                IndexingConstants.ModifiedUtcKey,
                contentItem.ModifiedUtc,
                DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                IndexingConstants.PublishedKey,
                contentItem.Published,
                DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                IndexingConstants.PublishedUtcKey,
                contentItem.PublishedUtc,
                DocumentIndexOptions.Store);

            return Task.CompletedTask;
        }
    }
}

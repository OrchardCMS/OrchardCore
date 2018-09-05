using System.Threading.Tasks;
using OrchardCore.Indexing;

namespace OrchardCore.Contents.Indexing
{
    public class DefaultContentIndexHandler : IContentItemIndexHandler
    {
        public Task BuildIndexAsync(BuildIndexContext context)
        {
            context.DocumentIndex.Entries.Add(
                IndexingConstants.ContentItemIdKey,
                new DocumentIndex.DocumentIndexEntry(
                    context.ContentItem.ContentItemId, 
                    DocumentIndex.Types.Text, 
                    DocumentIndexOptions.Store));

            context.DocumentIndex.Entries.Add(
                IndexingConstants.ContentItemVersionIdKey,
                new DocumentIndex.DocumentIndexEntry(
                    context.ContentItem.ContentItemVersionId,
                    DocumentIndex.Types.Text,
                    DocumentIndexOptions.Store));

            context.DocumentIndex.Entries.Add(
                IndexingConstants.ContentTypeKey,
                new DocumentIndex.DocumentIndexEntry(
                    context.ContentItem.ContentType,
                    DocumentIndex.Types.Text,
                    DocumentIndexOptions.Store));

            context.DocumentIndex.Entries.Add(
                IndexingConstants.CreatedUtcKey,
                new DocumentIndex.DocumentIndexEntry(
                    context.ContentItem.CreatedUtc,
                    DocumentIndex.Types.DateTime,
                    DocumentIndexOptions.Store));

            context.DocumentIndex.Entries.Add(
                IndexingConstants.LatestKey,
                new DocumentIndex.DocumentIndexEntry(
                    context.ContentItem.Latest,
                    DocumentIndex.Types.Boolean,
                    DocumentIndexOptions.Store));

            context.DocumentIndex.Entries.Add(
                IndexingConstants.OwnerKey,
                new DocumentIndex.DocumentIndexEntry(
                    context.ContentItem.Owner,
                    DocumentIndex.Types.Text,
                    DocumentIndexOptions.Store));

            context.DocumentIndex.Entries.Add(
                IndexingConstants.AuthorKey,
                new DocumentIndex.DocumentIndexEntry(
                    context.ContentItem.Author,
                    DocumentIndex.Types.Text,
                    DocumentIndexOptions.Store));

            context.DocumentIndex.Entries.Add(
                IndexingConstants.ModifiedUtcKey,
                new DocumentIndex.DocumentIndexEntry(
                    context.ContentItem.ModifiedUtc,
                    DocumentIndex.Types.DateTime,
                    DocumentIndexOptions.Store));

            context.DocumentIndex.Entries.Add(
                IndexingConstants.PublishedKey,
                new DocumentIndex.DocumentIndexEntry(
                    context.ContentItem.Published,
                    DocumentIndex.Types.Boolean,
                    DocumentIndexOptions.Store));

            context.DocumentIndex.Entries.Add(
                IndexingConstants.PublishedUtcKey,
                new DocumentIndex.DocumentIndexEntry(
                    context.ContentItem.PublishedUtc,
                    DocumentIndex.Types.DateTime,
                    DocumentIndexOptions.Store));

            return Task.CompletedTask;
        }
    }
}

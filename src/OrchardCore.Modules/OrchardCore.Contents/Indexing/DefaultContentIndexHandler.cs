using System.Threading.Tasks;
using OrchardCore.Indexing;

namespace OrchardCore.Contents.Indexing
{
    public class DefaultContentIndexHandler : IContentItemIndexHandler
    {
        public Task BuildIndexAsync(BuildIndexContext context)
        {
            context.DocumentIndex.Entries.Add(
                "Content.ContentItem.ContentItemId",
                new DocumentIndex.DocumentIndexEntry(
                    context.ContentItem.ContentItemId, 
                    DocumentIndex.Types.Text, 
                    DocumentIndexOptions.Store));

            context.DocumentIndex.Entries.Add(
                "Content.ContentItem.ContentItemVersionId",
                new DocumentIndex.DocumentIndexEntry(
                    context.ContentItem.ContentItemVersionId,
                    DocumentIndex.Types.Text,
                    DocumentIndexOptions.Store));

            context.DocumentIndex.Entries.Add(
                "Content.ContentItem.ContentType",
                new DocumentIndex.DocumentIndexEntry(
                    context.ContentItem.ContentType,
                    DocumentIndex.Types.Text,
                    DocumentIndexOptions.Store));

            context.DocumentIndex.Entries.Add(
                "Content.ContentItem.CreatedUtc",
                new DocumentIndex.DocumentIndexEntry(
                    context.ContentItem.CreatedUtc,
                    DocumentIndex.Types.DateTime,
                    DocumentIndexOptions.Store));

            context.DocumentIndex.Entries.Add(
                "Content.ContentItem.Latest",
                new DocumentIndex.DocumentIndexEntry(
                    context.ContentItem.Latest,
                    DocumentIndex.Types.Boolean,
                    DocumentIndexOptions.Store));

            context.DocumentIndex.Entries.Add(
                "Content.ContentItem.Owner",
                new DocumentIndex.DocumentIndexEntry(
                    context.ContentItem.Owner,
                    DocumentIndex.Types.Text,
                    DocumentIndexOptions.Store));

            context.DocumentIndex.Entries.Add(
                "Content.ContentItem.Author",
                new DocumentIndex.DocumentIndexEntry(
                    context.ContentItem.Author,
                    DocumentIndex.Types.Text,
                    DocumentIndexOptions.Store));

            context.DocumentIndex.Entries.Add(
                "Content.ContentItem.ModifiedUtc",
                new DocumentIndex.DocumentIndexEntry(
                    context.ContentItem.ModifiedUtc,
                    DocumentIndex.Types.DateTime,
                    DocumentIndexOptions.Store));

            context.DocumentIndex.Entries.Add(
                "Content.ContentItem.Published",
                new DocumentIndex.DocumentIndexEntry(
                    context.ContentItem.Published,
                    DocumentIndex.Types.Boolean,
                    DocumentIndexOptions.Store));

            context.DocumentIndex.Entries.Add(
                "Content.ContentItem.PublishedUtc",
                new DocumentIndex.DocumentIndexEntry(
                    context.ContentItem.PublishedUtc,
                    DocumentIndex.Types.DateTime,
                    DocumentIndexOptions.Store));

            return Task.CompletedTask;
        }
    }
}

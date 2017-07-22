using System.Threading.Tasks;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Models;
using Orchard.Indexing;

namespace Orchard.Contents.Indexing
{
    public class AspectsContentIndexHandler : IContentItemIndexHandler
    {
        private readonly IContentManager _contentManager;

        public AspectsContentIndexHandler(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public Task BuildIndexAsync(BuildIndexContext context)
        {
            var body = _contentManager.PopulateAspect(context.ContentItem, new BodyAspect());

            if (body != null)
            {
                context.DocumentIndex.Entries.Add(
                "Content.BodyAspect.Body",
                new DocumentIndex.DocumentIndexEntry(
                    body.Body,
                    DocumentIndex.Types.Text,
                    DocumentIndexOptions.Analyze | DocumentIndexOptions.Sanitize));
            }

            var contentItemMetadata = _contentManager.PopulateAspect<ContentItemMetadata>(context.ContentItem);

            if (contentItemMetadata?.DisplayText != null)
            {
                context.DocumentIndex.Entries.Add(
                "Content.ContentItemMetadata.DisplayText.Analyzed",
                new DocumentIndex.DocumentIndexEntry(
                    contentItemMetadata.DisplayText,
                    DocumentIndex.Types.Text,
                    DocumentIndexOptions.Analyze | DocumentIndexOptions.Sanitize));

                context.DocumentIndex.Entries.Add(
                "Content.ContentItemMetadata.DisplayText",
                new DocumentIndex.DocumentIndexEntry(
                    contentItemMetadata.DisplayText,
                    DocumentIndex.Types.Text,
                    DocumentIndexOptions.Store));
            }

            return Task.CompletedTask;
        }
    }
}

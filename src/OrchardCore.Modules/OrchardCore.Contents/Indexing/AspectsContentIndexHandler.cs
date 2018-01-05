using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Indexing;

namespace OrchardCore.Contents.Indexing
{
    public class AspectsContentIndexHandler : IContentItemIndexHandler
    {
        private readonly IContentManager _contentManager;

        public AspectsContentIndexHandler(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public async Task BuildIndexAsync(BuildIndexContext context)
        {
            var body = await _contentManager.PopulateAspectAsync(context.ContentItem, new BodyAspect());

            if (body != null)
            {
                context.DocumentIndex.Entries.Add(
                "Content.BodyAspect.Body",
                new DocumentIndex.DocumentIndexEntry(
                    body.Body,
                    DocumentIndex.Types.Text,
                    DocumentIndexOptions.Analyze | DocumentIndexOptions.Sanitize));
            }

            var contentItemMetadata = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(context.ContentItem);

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
        }
    }
}

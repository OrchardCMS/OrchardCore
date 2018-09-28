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

        public async Task BuildIndexAsync(BuildIndexContext context, ContentItem contentItem)
        {
            var body = await _contentManager.PopulateAspectAsync(contentItem, new BodyAspect());
            if (body != null)
            {
                context.DocumentIndex.Set(
                    IndexingConstants.BodyAspectBodyKey,
                    body.Body,
                    DocumentIndexOptions.Analyze | DocumentIndexOptions.Sanitize);
            }

            if (context.ContentItem.DisplayText != null)
            {
                context.DocumentIndex.Set(
                    IndexingConstants.DisplayTextAnalyzedKey,
                    contentItem.DisplayText,
                    DocumentIndexOptions.Analyze | DocumentIndexOptions.Sanitize);

                context.DocumentIndex.Set(
                    IndexingConstants.DisplayTextKey,
                    contentItem.DisplayText,
                    DocumentIndexOptions.Store);
            }
        }
    }
}

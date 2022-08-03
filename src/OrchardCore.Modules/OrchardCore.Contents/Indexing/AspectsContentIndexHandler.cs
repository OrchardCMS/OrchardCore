using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Indexing;
using OrchardCore.Mvc.Utilities;

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

            if (body != null && body.Body != null)
            {
                context.DocumentIndex.Set(
                    IndexingConstants.BodyAspectBodyKey,
                    body.Body,
                    DocumentIndexOptions.Analyze | DocumentIndexOptions.Sanitize);
            }

            context.DocumentIndex.Set(
                IndexingConstants.DisplayTextAnalyzedKey,
                context.ContentItem.DisplayText,
                DocumentIndexOptions.Analyze | DocumentIndexOptions.Sanitize);

            context.DocumentIndex.Set(
                IndexingConstants.DisplayTextKey,
                context.ContentItem.DisplayText,
                DocumentIndexOptions.Store);

            context.DocumentIndex.Set(
                IndexingConstants.DisplayTextNormalizedKey,
                context.ContentItem.DisplayText?.ReplaceDiacritics().ToLower(),
                DocumentIndexOptions.Store);
        }
    }
}

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
                IndexingConstants.BodyAspectBodyKey,
                new DocumentIndex.DocumentIndexEntry(
                    body.Body,
                    DocumentIndex.Types.Text,
                    DocumentIndexOptions.Analyze | DocumentIndexOptions.Sanitize));
            }

            if (context.ContentItem.DisplayText != null)
            {
                context.DocumentIndex.Entries.Add(
                IndexingConstants.DisplayTextAnalyzedKey,
                new DocumentIndex.DocumentIndexEntry(
                    context.ContentItem.DisplayText,
                    DocumentIndex.Types.Text,
                    DocumentIndexOptions.Analyze | DocumentIndexOptions.Sanitize));

                context.DocumentIndex.Entries.Add(
                IndexingConstants.DisplayTextKey,
                new DocumentIndex.DocumentIndexEntry(
                    context.ContentItem.DisplayText,
                    DocumentIndex.Types.Text,
                    DocumentIndexOptions.Store));
            }
        }
    }
}

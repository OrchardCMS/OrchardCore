using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Indexing;

namespace OrchardCore.Contents.Indexing
{
    public class FullTextContentIndexHandler : IContentItemIndexHandler
    {
        private readonly IContentManager _contentManager;

        public FullTextContentIndexHandler(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public async Task BuildIndexAsync(BuildIndexContext context)
        {
            var result = await _contentManager.PopulateAspectAsync<FullTextAspect>(context.ContentItem);

            if (result.Indexed)
            {
                context.DocumentIndex.Set(
                    IndexingConstants.FullTextKey,
                    result.FullText.ToString(),
                    DocumentIndexOptions.Analyze | DocumentIndexOptions.Sanitize);
            }
        }
    }
}

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

            // Index each segment as a new value to prevent from allocation a new string
            foreach (var segment in result.Segments)
            {
                context.DocumentIndex.Set(
                    IndexingConstants.FullTextKey,
                    segment,
                    DocumentIndexOptions.Analyze | DocumentIndexOptions.Sanitize);
            }
        }
    }
}

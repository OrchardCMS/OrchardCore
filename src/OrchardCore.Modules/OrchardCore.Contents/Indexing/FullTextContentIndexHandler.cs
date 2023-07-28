using System;
using System.Threading.Tasks;
using Cysharp.Text;
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

            using var stringBuilder = ZString.CreateStringBuilder();

            foreach (var segment in result.Segments)
            {
                stringBuilder.Append(segment + " ");
            }

            if (!String.IsNullOrEmpty(stringBuilder.ToString()))
            {
                context.DocumentIndex.Set(
                    IndexingConstants.FullTextKey,
                    stringBuilder.ToString(),
                    DocumentIndexOptions.Sanitize);
            }
        }
    }
}

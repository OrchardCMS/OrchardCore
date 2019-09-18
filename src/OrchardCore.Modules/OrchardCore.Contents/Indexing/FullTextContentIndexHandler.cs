using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Indexing;

namespace OrchardCore.Contents.Indexing
{
    public class FullTextContentIndexHandler : IContentItemIndexHandler
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;

        public FullTextContentIndexHandler(
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
        }

        public async Task BuildIndexAsync(BuildIndexContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);

            if (contentTypeDefinition == null)
            {
                return;
            }

            var settings = contentTypeDefinition.GetSettings<ContentTypeIndexingSettings>();

            if (settings.IsFullText && !String.IsNullOrEmpty(settings.FullText))
            {
                var result = await _contentManager.PopulateAspectAsync(context.ContentItem, new FullTextAspect { FullText = settings.FullText });

                context.DocumentIndex.Set(
                    IndexingConstants.FullTextKey,
                    result.FullText,
                    DocumentIndexOptions.Analyze);
            }
        }
    }
}

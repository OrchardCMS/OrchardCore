using System;
using System.Threading.Tasks;
using Fluid;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Indexing;
using OrchardCore.Liquid;

namespace OrchardCore.Contents.Indexing
{
    public class FullTextContentIndexHandler : IContentItemIndexHandler
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        public const string FullTextKey = "Content.ContentItem.FullText";

        public FullTextContentIndexHandler(
            IContentDefinitionManager contentDefinitionManager,
            ILiquidTemplateManager liquidTemplateManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _liquidTemplateManager = liquidTemplateManager;
        }

        public async Task BuildIndexAsync(BuildIndexContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);

            if (contentTypeDefinition == null)
            {
                return;
            }

            var settings = contentTypeDefinition.Settings.ToObject<ContentTypeIndexingSettings>();

            if (settings.IsFullText && !String.IsNullOrEmpty(settings.FullText))
            {
                var templateContext = new TemplateContext();
                templateContext.SetValue("Model", context.ContentItem);

                var result = await _liquidTemplateManager.RenderAsync(settings.FullText, NullEncoder.Default, templateContext);

                context.DocumentIndex.Set(
                    FullTextKey,
                    result,
                    DocumentIndexOptions.Analyze);
            }
        }
    }
}

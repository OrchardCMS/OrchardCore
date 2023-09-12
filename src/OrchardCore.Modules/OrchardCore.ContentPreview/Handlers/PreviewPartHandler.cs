using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentPreview.Models;
using OrchardCore.ContentPreview.ViewModels;
using OrchardCore.Liquid;

namespace OrchardCore.ContentPreview.Handlers
{
    public class PreviewPartHandler : ContentPartHandler<PreviewPart>
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public PreviewPartHandler(
            ILiquidTemplateManager liquidTemplateManager,
            IContentDefinitionManager contentDefinitionManager)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _contentDefinitionManager = contentDefinitionManager;
        }

        /// <summary>
        /// Get the pattern from the AutoroutePartSettings property for its type
        /// </summary>
        private string GetPattern(PreviewPart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, "PreviewPart"));
            var pattern = contentTypePartDefinition.GetSettings<PreviewPartSettings>().Pattern;

            return pattern;
        }

        public override async Task GetContentItemAspectAsync(ContentItemAspectContext context, PreviewPart part)
        {
            var pattern = GetPattern(part);

            if (!String.IsNullOrEmpty(pattern))
            {
                await context.ForAsync<PreviewAspect>(async previewAspect =>
                {
                    var model = new PreviewPartViewModel()
                    {
                        PreviewPart = part,
                        ContentItem = part.ContentItem,
                    };

                    previewAspect.PreviewUrl = await _liquidTemplateManager.RenderStringAsync(pattern, NullEncoder.Default, model,
                        new Dictionary<string, FluidValue>() { ["ContentItem"] = new ObjectValue(model.ContentItem) });

                    previewAspect.PreviewUrl = previewAspect.PreviewUrl.Replace("\r", String.Empty).Replace("\n", String.Empty);
                });
            }

            return;
        }
    }
}

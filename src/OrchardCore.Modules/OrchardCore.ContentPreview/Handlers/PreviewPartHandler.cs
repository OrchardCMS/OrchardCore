using System;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Routing;
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
                string previewUrl = null;

                var model = new PreviewPartViewModel()
                {
                    PreviewPart = part,
                    ContentItem = part.ContentItem
                };

                previewUrl = await _liquidTemplateManager.RenderAsync(pattern, NullEncoder.Default, model,
                    scope => scope.SetValue("ContentItem", model.ContentItem));

                previewUrl = previewUrl.Replace("\r", String.Empty).Replace("\n", String.Empty);

                await context.ForAsync<PreviewAspect>(previewAspect =>
                 {
                     previewAspect.PreviewUrl = previewUrl;
                     return Task.CompletedTask;
                 });
                await context.ForAsync<ContentItemMetadata>(metadata =>
                {
                    if (metadata.DisplayRouteValues == null)
                    {
                        metadata.DisplayRouteValues = new RouteValueDictionary {
                            {"PreviewUrl", previewUrl}
                        };
                    }
                    else
                    {
                        metadata.DisplayRouteValues.Add("PreviewUrl", previewUrl);
                    }
                    return Task.CompletedTask;
                });
            }

            return;
        }
    }
}

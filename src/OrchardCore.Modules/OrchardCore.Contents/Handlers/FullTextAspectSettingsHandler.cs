using System;
using System.IO;
using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Contents.Models;
using OrchardCore.Liquid;

namespace OrchardCore.Contents.Handlers
{
    /// <summary>
    /// Provides the content for FullTextAspect based on FullTextAspectSettings
    /// </summary>
    public class FullTextAspectSettingsHandler : ContentHandlerBase
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IServiceProvider _serviceProvider;

        public FullTextAspectSettingsHandler(
            IContentDefinitionManager contentDefinitionManager,
            ILiquidTemplateManager liquidTemplateManager,
            IServiceProvider serviceProvider
            )
        {
            _contentDefinitionManager = contentDefinitionManager;
            _liquidTemplateManager = liquidTemplateManager;
            _serviceProvider = serviceProvider;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);

            if (contentTypeDefinition == null)
            {
                return Task.CompletedTask;
            }

            return context.ForAsync<FullTextAspect>(async fullTextAspect =>
            {
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
                var settings = contentTypeDefinition.GetSettings<FullTextAspectSettings>();

                if (settings.IncludeDisplayText)
                {
                    fullTextAspect.Segments.Add(context.ContentItem.DisplayText);
                }

                if (settings.IncludeBodyAspect)
                {
                    // Lazy resolution to prevent cyclic dependency of content handlers
                    var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
                    var bodyAspect = await contentManager.PopulateAspectAsync<BodyAspect>(context.ContentItem);

                    if (bodyAspect != null && bodyAspect.Body != null)
                    {
                        using (var sw = new StringWriter())
                        {
                            // Don't encode the body
                            bodyAspect.Body.WriteTo(sw, NullHtmlEncoder.Default);
                            fullTextAspect.Segments.Add(sw.ToString());
                        }
                    }
                }

                if (settings.IncludeFullTextTemplate && !String.IsNullOrEmpty(settings.FullTextTemplate))
                {
                    var result = await _liquidTemplateManager.RenderAsync(settings.FullTextTemplate, NullEncoder.Default, context.ContentItem,
                        scope => scope.SetValue("ContentItem", context.ContentItem));

                    fullTextAspect.Segments.Add(result);
                }
            });
        }
    }
}

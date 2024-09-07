using Cysharp.Text;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Contents.Models;
using OrchardCore.Liquid;

namespace OrchardCore.Contents.Handlers;

/// <summary>
/// Provides the content for FullTextAspect based on FullTextAspectSettings.
/// </summary>
public class FullTextAspectContentHandler : ContentHandlerBase
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly IServiceProvider _serviceProvider;

    public FullTextAspectContentHandler(
        IContentDefinitionManager contentDefinitionManager,
        ILiquidTemplateManager liquidTemplateManager,
        IServiceProvider serviceProvider
        )
    {
        _contentDefinitionManager = contentDefinitionManager;
        _liquidTemplateManager = liquidTemplateManager;
        _serviceProvider = serviceProvider;
    }

    public override async Task GetContentItemAspectAsync(ContentItemAspectContext context)
    {
        var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(context.ContentItem.ContentType);

        if (contentTypeDefinition == null)
        {
            return;
        }

        await context.ForAsync<FullTextAspect>(async fullTextAspect =>
        {
            var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(context.ContentItem.ContentType);
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
                    using var sw = new ZStringWriter();
                    // Don't encode the body
                    bodyAspect.Body.WriteTo(sw, NullHtmlEncoder.Default);
                    fullTextAspect.Segments.Add(sw.ToString());
                }
            }

            if (settings.IncludeFullTextTemplate && !string.IsNullOrEmpty(settings.FullTextTemplate))
            {
                var result = await _liquidTemplateManager.RenderStringAsync(settings.FullTextTemplate, NullEncoder.Default, context.ContentItem,
                    new Dictionary<string, FluidValue>() { ["ContentItem"] = new ObjectValue(context.ContentItem) });

                fullTextAspect.Segments.Add(result);
            }
        });
    }
}

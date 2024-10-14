using Fluid;
using Fluid.Values;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentPreview.Models;
using OrchardCore.ContentPreview.ViewModels;
using OrchardCore.Liquid;

namespace OrchardCore.ContentPreview.Handlers;

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
    /// Get the pattern from the AutoroutePartSettings property for its type.
    /// </summary>
    private async Task<string> GetPatternAsync(PreviewPart part)
    {
        var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(part.ContentItem.ContentType);
        var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => string.Equals(x.PartDefinition.Name, "PreviewPart", StringComparison.Ordinal));
        var pattern = contentTypePartDefinition.GetSettings<PreviewPartSettings>().Pattern;

        return pattern;
    }

    public override async Task GetContentItemAspectAsync(ContentItemAspectContext context, PreviewPart part)
    {
        var pattern = await GetPatternAsync(part);

        if (!string.IsNullOrEmpty(pattern))
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

                previewAspect.PreviewUrl = previewAspect.PreviewUrl.Replace("\r", string.Empty).Replace("\n", string.Empty);
            });
        }

        return;
    }
}

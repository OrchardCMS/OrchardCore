using System.Text.Encodings.Web;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Html.Models;
using OrchardCore.Html.Settings;
using OrchardCore.Html.ViewModels;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Shortcodes.Services;
using Shortcodes;

namespace OrchardCore.Html.Handlers;

public class HtmlBodyPartHandler : ContentPartHandler<HtmlBodyPart>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IShortcodeService _shortcodeService;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly HtmlEncoder _htmlEncoder;
    private readonly IHtmlSanitizerService _htmlSanitizerService;

    public HtmlBodyPartHandler(IContentDefinitionManager contentDefinitionManager,
        IShortcodeService shortcodeService,
        ILiquidTemplateManager liquidTemplateManager,
        HtmlEncoder htmlEncoder,
        IHtmlSanitizerService htmlSanitizerService)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _shortcodeService = shortcodeService;
        _liquidTemplateManager = liquidTemplateManager;
        _htmlEncoder = htmlEncoder;
        _htmlSanitizerService = htmlSanitizerService;
    }

    public override Task GetContentItemAspectAsync(ContentItemAspectContext context, HtmlBodyPart part)
    {
        return context.ForAsync<BodyAspect>(async bodyAspect =>
        {
            try
            {
                var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(part.ContentItem.ContentType);
                var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => string.Equals(x.PartDefinition.Name, "HtmlBodyPart", StringComparison.Ordinal));
                var settings = contentTypePartDefinition.GetSettings<HtmlBodyPartSettings>();

                var html = part.Html;

                if (settings.RenderLiquid)
                {
                    var model = new HtmlBodyPartViewModel()
                    {
                        Html = part.Html,
                        HtmlBodyPart = part,
                        ContentItem = part.ContentItem,
                    };

                    html = await _liquidTemplateManager.RenderStringAsync(html, _htmlEncoder, model,
                        new Dictionary<string, FluidValue>() { ["ContentItem"] = new ObjectValue(model.ContentItem) });
                }

                html = await _shortcodeService.ProcessAsync(html,
                    new Context
                    {
                        ["ContentItem"] = part.ContentItem,
                        ["TypePartDefinition"] = contentTypePartDefinition,
                    });

                bodyAspect.Body = new HtmlString(html);
            }
            catch
            {
                bodyAspect.Body = HtmlString.Empty;
            }
        });
    }

    public override async Task ImportedAsync(ImportContentContext context, HtmlBodyPart part)
    {
        var typeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(context.ContentItem.ContentType);

        if (typeDefinition.GetSettings<HtmlBodyPartSettings>() is { SanitizeHtml: true })
        {
            part.Html = _htmlSanitizerService.Sanitize(part.Html);
        }
    }
}

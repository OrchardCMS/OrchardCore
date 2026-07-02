using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Menu.Models;
using OrchardCore.Menu.Settings;

namespace OrchardCore.Menu.Handlers;

public class HtmlMenuItemPartHandler : ContentPartHandler<HtmlMenuItemPart>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IHtmlSanitizerService _htmlSanitizerService;
    public HtmlMenuItemPartHandler(IContentDefinitionManager contentDefinitionManager, IHtmlSanitizerService htmlSanitizerService)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _htmlSanitizerService = htmlSanitizerService;
    }
    
    public override async Task ImportedAsync(ImportContentContext context, HtmlMenuItemPart part)
    {
        var typeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(context.ContentItem.ContentType);

        if (typeDefinition.GetSettings<HtmlMenuItemPartSettings>() is { SanitizeHtml: true })
        {
            context.ContentItem.Alter<HtmlMenuItemPart>(part => 
                part.Html = _htmlSanitizerService.Sanitize(part.Html));
        }
    }
}

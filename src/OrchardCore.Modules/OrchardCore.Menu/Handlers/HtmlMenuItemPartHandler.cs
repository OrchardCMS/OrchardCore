using OrchardCore.ContentManagement.Extensions;
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
    
    public override Task ImportedAsync(ImportContentContext context, HtmlMenuItemPart part) =>
        _contentDefinitionManager.SanitizeHtmlHolderAsync(
            _htmlSanitizerService,
            part,
            (definition, _) => definition.GetSettings<HtmlMenuItemPartSettings>() is { SanitizeHtml: true });
}

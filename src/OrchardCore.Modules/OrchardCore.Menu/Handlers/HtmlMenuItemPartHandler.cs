using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Menu.Models;

namespace OrchardCore.Menu.Handlers;

[Obsolete("This handler is no longer used.")]
public class HtmlMenuItemPartHandler : ContentPartHandler<HtmlMenuItemPart>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IHtmlSanitizerService _htmlSanitizerService;
    public HtmlMenuItemPartHandler(IContentDefinitionManager contentDefinitionManager, IHtmlSanitizerService htmlSanitizerService)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _htmlSanitizerService = htmlSanitizerService;
    }
    
    public override Task ImportedAsync(ImportContentContext context, HtmlMenuItemPart part)
    {
        // The old handler logic has been removed but the method is kept for binary backwards compatibility.
        return Task.CompletedTask;
    }
}

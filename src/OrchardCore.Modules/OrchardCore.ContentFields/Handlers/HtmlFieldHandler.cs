using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Infrastructure.Html;

namespace OrchardCore.ContentFields.Handlers;

[Obsolete("This handler is no longer used.")]
public class HtmlFieldHandler : ContentFieldHandler<HtmlField>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IHtmlSanitizerService _htmlSanitizerService;

    public HtmlFieldHandler(
        IContentDefinitionManager contentDefinitionManager,
        IHtmlSanitizerService htmlSanitizerService)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _htmlSanitizerService = htmlSanitizerService;
    }

    public override Task ImportedAsync(ImportContentFieldContext context, HtmlField field)
    {
        // The old handler logic has been removed but the method is kept for binary backwards compatibility.
        return Task.CompletedTask;
    }
}

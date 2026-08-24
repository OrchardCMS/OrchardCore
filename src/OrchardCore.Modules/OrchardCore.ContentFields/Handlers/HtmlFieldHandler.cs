using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Infrastructure.Html;

namespace OrchardCore.ContentFields.Handlers;

[Obsolete("This handler is no longer used.")]
public class HtmlFieldHandler : ContentFieldHandler<HtmlField>
{
    public HtmlFieldHandler(
        IContentDefinitionManager contentDefinitionManager,
        IHtmlSanitizerService htmlSanitizerService)
    {
    }

    public override Task ImportedAsync(ImportContentFieldContext context, HtmlField field)
    {
        // The old handler logic has been removed but the method is kept for binary backwards compatibility.
        return Task.CompletedTask;
    }
}

using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Infrastructure.Html;

namespace OrchardCore.ContentFields.Handlers;

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
        var settings = context.ContentPartFieldDefinition.GetSettings<HtmlFieldSettings>();

        if (settings?.SanitizeHtml == true)
        {
context.ContentItem.Content[context.ContentPartFieldDefinition.PartDefinition.Name][context.ContentPartFieldDefinition.Name].Html =
                _htmlSanitizerService.Sanitize(field.Html);
        }

        return Task.CompletedTask;
    }
}

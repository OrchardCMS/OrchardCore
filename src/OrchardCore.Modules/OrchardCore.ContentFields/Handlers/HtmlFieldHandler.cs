using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Extensions;
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

    public override Task ImportedAsync(ImportContentFieldContext context, HtmlField field) =>
        _contentDefinitionManager.SanitizeHtmlHolderAsync(
            _htmlSanitizerService,
            field,
            (definition, _) => definition.GetSettings<HtmlFieldSettings>() is { SanitizeHtml: true });
}

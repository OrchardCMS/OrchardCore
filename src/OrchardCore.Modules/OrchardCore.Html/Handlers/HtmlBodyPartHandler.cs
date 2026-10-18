using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Html.Models;
using OrchardCore.Html.Services;
using OrchardCore.Html.Settings;
using OrchardCore.Html.ViewModels;
using Shortcodes;

namespace OrchardCore.Html.Handlers;

public class HtmlBodyPartHandler : ContentPartHandler<HtmlBodyPart>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IHtmlDisplayService _htmlDisplayService;

    public HtmlBodyPartHandler(
        IContentDefinitionManager contentDefinitionManager,
        IHtmlDisplayService htmlDisplayService)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _htmlDisplayService = htmlDisplayService;
    }

    public override Task GetContentItemAspectAsync(ContentItemAspectContext context, HtmlBodyPart part)
    {
        return context.ForAsync<BodyAspect>(async bodyAspect =>
        {
            try
            {
                var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(part.ContentItem.ContentType);
                var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => string.Equals(x.PartDefinition.Name, "HtmlBodyPart", StringComparison.Ordinal));
                var settings = contentTypePartDefinition?.GetSettings<HtmlBodyPartSettings>() ?? new();

                var model = new HtmlBodyPartViewModel
                {
                    Html = part.Html,
                    HtmlBodyPart = part,
                    ContentItem = part.ContentItem,
                    TypePartDefinition = contentTypePartDefinition,
                };

                await _htmlDisplayService.UpdateModelHtmlAsync(
                    model,
                    new Context
                    {
                        ["TypePartDefinition"] = contentTypePartDefinition,
                    },
                    settings.SanitizeHtml);

                bodyAspect.Body = new HtmlString(model.Html);
            }
            catch
            {
                bodyAspect.Body = HtmlString.Empty;
            }
        });
    }
}

using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Markdown.Models;
using OrchardCore.Markdown.Services;
using OrchardCore.Markdown.Settings;
using Shortcodes;

namespace OrchardCore.Markdown.Handlers;

public class MarkdownBodyPartHandler : ContentPartHandler<MarkdownBodyPart>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IMarkdownDisplayService _markdownDisplayService;

    public MarkdownBodyPartHandler(
        IContentDefinitionManager contentDefinitionManager,
        IMarkdownDisplayService markdownDisplayService)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _markdownDisplayService = markdownDisplayService;
    }

    public override Task GetContentItemAspectAsync(ContentItemAspectContext context, MarkdownBodyPart part)
    {
        return context.ForAsync<BodyAspect>(async bodyAspect =>
        {
            try
            {
                var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(part.ContentItem.ContentType);
                var contentTypePartDefinition = contentTypeDefinition.Parts
                    .FirstOrDefault(x => string.Equals(x.PartDefinition.Name, "MarkdownBodyPart", StringComparison.Ordinal));
                var settings = contentTypePartDefinition.GetSettings<MarkdownBodyPartSettings>();

                var html = await _markdownDisplayService.ToHtmlAsync(
                    part.Markdown,
                    new Context
                    {
                        ["ContentItem"] = part.ContentItem,
                        ["TypePartDefinition"] = contentTypePartDefinition,
                    },
                    settings.SanitizeHtml);

                bodyAspect.Body = new HtmlString(html);
            }
            catch
            {
                bodyAspect.Body = HtmlString.Empty;
            }
        });
    }
}

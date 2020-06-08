using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Models;
using OrchardCore.ShortCodes.Services;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Models;
using OrchardCore.Markdown.Services;
using OrchardCore.Markdown.Settings;
using OrchardCore.Markdown.ViewModels;

namespace OrchardCore.Markdown.Handlers
{
    public class MarkdownBodyPartHandler : ContentPartHandler<MarkdownBodyPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IShortCodeService _shortCodeService;
        private readonly IMarkdownService _markdownService;
        private readonly IHtmlSanitizerService _htmlSanitizerService;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly HtmlEncoder _htmlEncoder;
        private HtmlString _bodyAspect;
        private int _contentItemId;

        public MarkdownBodyPartHandler(IContentDefinitionManager contentDefinitionManager,
            IShortCodeService shortCodeService,
            IMarkdownService markdownService,
            IHtmlSanitizerService htmlSanitizerService,
            ILiquidTemplateManager liquidTemplateManager,
            HtmlEncoder htmlEncoder)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _shortCodeService = shortCodeService;
            _markdownService = markdownService;
            _htmlSanitizerService = htmlSanitizerService;
            _liquidTemplateManager = liquidTemplateManager;
            _htmlEncoder = htmlEncoder;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, MarkdownBodyPart part)
        {
            return context.ForAsync<BodyAspect>(async bodyAspect =>
            {
                if (bodyAspect != null && part.ContentItem.Id == _contentItemId)
                {
                    bodyAspect.Body = _bodyAspect;

                    return;
                }

                try
                {
                    var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
                    var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, "MarkdownBodyPart"));
                    var settings = contentTypePartDefinition.GetSettings<MarkdownBodyPartSettings>();

                    // The default Markdown option is to entity escape html
                    // so filters must be run after the markdown has been processed.
                    var html = _markdownService.ToHtml(part.Markdown);

                    // The liquid rendering is for backwards compatability and can be removed in a future version.
                    if (!settings.SanitizeHtml)
                    {
                        var model = new MarkdownBodyPartViewModel()
                        {
                            Markdown = part.Markdown,
                            Html = html,
                            MarkdownBodyPart = part,
                            ContentItem = part.ContentItem
                        };

                        html = await _liquidTemplateManager.RenderAsync(html, _htmlEncoder, model,
                            scope => scope.SetValue("ContentItem", model.ContentItem));
                    }

                    html = await _shortCodeService.ProcessAsync(html);

                    if (settings.SanitizeHtml)
                    {
                        html = _htmlSanitizerService.Sanitize(html);
                    }

                    bodyAspect.Body = _bodyAspect = new HtmlString(html);
                    _contentItemId = part.ContentItem.Id;
                }
                catch
                {
                    bodyAspect.Body = HtmlString.Empty;
                    _contentItemId = default;
                }
            });
        }
    }
}

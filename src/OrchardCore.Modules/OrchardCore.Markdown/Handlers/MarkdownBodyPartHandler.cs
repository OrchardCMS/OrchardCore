using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Models;
using OrchardCore.Markdown.Services;
using OrchardCore.Markdown.Settings;
using OrchardCore.Markdown.ViewModels;
using OrchardCore.Shortcodes.Services;
using Shortcodes;

namespace OrchardCore.Markdown.Handlers
{
    public class MarkdownBodyPartHandler : ContentPartHandler<MarkdownBodyPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IShortcodeService _shortcodeService;
        private readonly IMarkdownService _markdownService;
        private readonly IHtmlSanitizerService _htmlSanitizerService;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly HtmlEncoder _htmlEncoder;

        public MarkdownBodyPartHandler(IContentDefinitionManager contentDefinitionManager,
            IShortcodeService shortcodeService,
            IMarkdownService markdownService,
            IHtmlSanitizerService htmlSanitizerService,
            ILiquidTemplateManager liquidTemplateManager,
            HtmlEncoder htmlEncoder)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _shortcodeService = shortcodeService;
            _markdownService = markdownService;
            _htmlSanitizerService = htmlSanitizerService;
            _liquidTemplateManager = liquidTemplateManager;
            _htmlEncoder = htmlEncoder;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, MarkdownBodyPart part)
        {
            return context.ForAsync<BodyAspect>(async bodyAspect =>
            {
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
                            ContentItem = part.ContentItem,
                        };

                        html = await _liquidTemplateManager.RenderStringAsync(html, _htmlEncoder, model,
                            new Dictionary<string, FluidValue>() { ["ContentItem"] = new ObjectValue(model.ContentItem) });
                    }

                    html = await _shortcodeService.ProcessAsync(html,
                        new Context
                        {
                            ["ContentItem"] = part.ContentItem,
                            ["TypePartDefinition"] = contentTypePartDefinition
                        });

                    if (settings.SanitizeHtml)
                    {
                        html = _htmlSanitizerService.Sanitize(html);
                    }

                    bodyAspect.Body = new HtmlString(html);
                }
                catch
                {
                    bodyAspect.Body = HtmlString.Empty;
                }
            });
        }
    }
}

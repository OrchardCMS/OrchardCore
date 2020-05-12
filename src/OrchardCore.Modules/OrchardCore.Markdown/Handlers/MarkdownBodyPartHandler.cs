using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Infrastructure.SafeCodeFilters;
using OrchardCore.Infrastructure.Script;
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
        private readonly ISafeCodeFilterManager _safeCodeFilterManager;
        private readonly IMarkdownService _markdownService;
        private readonly IHtmlScriptSanitizer _htmlScriptSanitizer;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly HtmlEncoder _htmlEncoder;
        private HtmlString _bodyAspect;
        private int _contentItemId;

        public MarkdownBodyPartHandler(IContentDefinitionManager contentDefinitionManager,
            ISafeCodeFilterManager safeCodeFilterManager,
            IMarkdownService markdownService,
            IHtmlScriptSanitizer htmlScriptSanitizer,
            ILiquidTemplateManager liquidTemplateManager,
            HtmlEncoder htmlEncoder)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _safeCodeFilterManager = safeCodeFilterManager;
            _markdownService = markdownService;
            _htmlScriptSanitizer = htmlScriptSanitizer;
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

                    var markdown = part.Markdown;

                    if (settings.AllowCustomScripts)
                    {
                        var model = new MarkdownBodyPartViewModel()
                        {
                            Markdown = part.Markdown,
                            MarkdownBodyPart = part,
                            ContentItem = part.ContentItem
                        };

                        markdown = await _liquidTemplateManager.RenderAsync(markdown, _htmlEncoder, model,
                            scope => scope.SetValue("ContentItem", model.ContentItem));
                    }

                    markdown = await _safeCodeFilterManager.ProcessAsync(markdown);
                    markdown = _markdownService.ToHtml(markdown);

                    if (!settings.AllowCustomScripts)
                    {
                        markdown = _htmlScriptSanitizer.Sanitize(markdown);
                    }

                    bodyAspect.Body = _bodyAspect = new HtmlString(markdown);
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

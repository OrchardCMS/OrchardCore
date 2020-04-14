using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Models;
using OrchardCore.Markdown.ViewModels;

namespace OrchardCore.Markdown.Handlers
{
    public class MarkdownBodyPartHandler : ContentPartHandler<MarkdownBodyPart>
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly IDictionary<string, HtmlString> _bodiesAspectDictionary = new Dictionary<string, HtmlString>();

        public MarkdownBodyPartHandler(ILiquidTemplateManager liquidTemplateManager, HtmlEncoder htmlEncoder)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _htmlEncoder = htmlEncoder;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, MarkdownBodyPart part)
        {
            return context.ForAsync<BodyAspect>(async bodyAspect =>
            {
                var contentItemVersionId = part.ContentItem.ContentItemVersionId;
                if (_bodiesAspectDictionary.ContainsKey(contentItemVersionId))
                {
                    bodyAspect.Body = _bodiesAspectDictionary[contentItemVersionId];

                    return;
                }

                try
                {
                    var model = new MarkdownBodyPartViewModel()
                    {
                        Markdown = part.Markdown,
                        MarkdownBodyPart = part,
                        ContentItem = part.ContentItem
                    };

                    var markdown = await _liquidTemplateManager.RenderAsync(part.Markdown, _htmlEncoder, model,
                        scope => scope.SetValue("ContentItem", model.ContentItem));

                    var result = Markdig.Markdown.ToHtml(markdown ?? "");

                    _bodiesAspectDictionary.Add(contentItemVersionId, new HtmlString(result));
                    bodyAspect.Body = _bodiesAspectDictionary[contentItemVersionId];
                }
                catch
                {
                    bodyAspect.Body = HtmlString.Empty;
                }
            });
        }
    }
}

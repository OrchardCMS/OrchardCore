using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
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
        private HtmlString _bodyAspect;

        public MarkdownBodyPartHandler(ILiquidTemplateManager liquidTemplateManager, HtmlEncoder htmlEncoder)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _htmlEncoder = htmlEncoder;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, MarkdownBodyPart part)
        {
            return context.ForAsync<BodyAspect>(async bodyAspect =>
            {
                if (_bodyAspect != null)
                {
                    bodyAspect.Body = _bodyAspect;
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

                    var templateContext = new TemplateContext();
                    templateContext.SetValue("ContentItem", part.ContentItem);
                    templateContext.MemberAccessStrategy.Register<MarkdownBodyPartViewModel>();
                    templateContext.SetValue("Model", model);

                    var markdown = await _liquidTemplateManager.RenderAsync(part.Markdown, _htmlEncoder, templateContext);
                    var result = Markdig.Markdown.ToHtml(markdown ?? "");

                    bodyAspect.Body = _bodyAspect = new HtmlString(result);
                }
                catch
                {
                    bodyAspect.Body = HtmlString.Empty;
                }
            });
        }
    }
}

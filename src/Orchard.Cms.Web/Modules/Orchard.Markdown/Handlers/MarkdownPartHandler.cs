using Microsoft.AspNetCore.Html;
using Orchard.Markdown.Model;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Models;

namespace Orchard.Markdown.Handlers
{
    public class MarkdownPartHandler : ContentPartHandler<MarkdownPart>
    {
        public override void GetContentItemAspect(ContentItemAspectContext context, MarkdownPart part)
        {
            context.For<BodyAspect>(bodyAspect => bodyAspect.Body = new HtmlString(part.Markdown));
        }
    }
}
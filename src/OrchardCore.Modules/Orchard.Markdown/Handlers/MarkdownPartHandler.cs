using System.Linq;
using Microsoft.AspNetCore.Html;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Models;
using Orchard.Markdown.Model;
using Orchard.Markdown.Settings;

namespace Orchard.Markdown.Handlers
{
    public class MarkdownPartHandler : ContentPartHandler<MarkdownPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public MarkdownPartHandler(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override void GetContentItemAspect(ContentItemAspectContext context, MarkdownPart part)
        {
            context.For<BodyAspect>(bodyAspect =>
            {

                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
                var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(MarkdownPart));
                var settings = contentTypePartDefinition.GetSettings<MarkdownPartSettings>();

                var html = Markdig.Markdown.ToHtml(part.Markdown ?? "");

                bodyAspect.Body = new HtmlString(html);
            });
        }
    }
}
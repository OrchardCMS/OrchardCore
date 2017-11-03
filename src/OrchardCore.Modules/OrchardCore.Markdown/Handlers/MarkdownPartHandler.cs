using System.Linq;
using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Markdown.Model;
using OrchardCore.Markdown.Settings;

namespace OrchardCore.Markdown.Handlers
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
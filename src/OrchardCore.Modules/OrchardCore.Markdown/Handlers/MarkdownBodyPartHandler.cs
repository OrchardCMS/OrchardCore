using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Markdown.Model;
using OrchardCore.Markdown.Settings;

namespace OrchardCore.Markdown.Handlers
{
    public class MarkdownBodyPartHandler : ContentPartHandler<MarkdownBodyPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public MarkdownBodyPartHandler(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, MarkdownBodyPart part)
        {
            return context.ForAsync<BodyAspect>(bodyAspect =>
            {
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
                var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(MarkdownBodyPart));
                var settings = contentTypePartDefinition.GetSettings<MarkdownBodyPartSettings>();
                var html = Markdig.Markdown.ToHtml(part.Markdown ?? "");

                bodyAspect.Body = new HtmlString(html);
                return Task.CompletedTask;
            });
        }
    }
}

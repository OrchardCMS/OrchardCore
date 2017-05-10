using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Models;
using Orchard.Markdown.Model;
using Orchard.Markdown.Settings;
using Orchard.Tokens.Services;

namespace Orchard.Markdown.Handlers
{
    public class MarkdownPartHandler : ContentPartHandler<MarkdownPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITokenizer _tokenizer;

        public MarkdownPartHandler(
            IContentDefinitionManager contentDefinitionManager,
            ITokenizer tokenizer
            )
        {
            _contentDefinitionManager = contentDefinitionManager;
            _tokenizer = tokenizer;
        }

        public override void GetContentItemAspect(ContentItemAspectContext context, MarkdownPart part)
        {
            context.For<BodyAspect>(bodyAspect =>
            {

                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
                var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(MarkdownPart));
                var settings = contentTypePartDefinition.GetSettings<MarkdownPartSettings>();

                var html = Markdig.Markdown.ToHtml(part.Markdown);

                if (settings.RenderTokens)
                {
                    html = _tokenizer.Tokenize(html, new Dictionary<string, object> { ["Content"] = part.ContentItem });
                }

                bodyAspect.Body = new HtmlString(html);
            });
        }
    }
}
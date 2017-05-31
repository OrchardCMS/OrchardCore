using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Orchard.Body.Model;
using Orchard.Body.Settings;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Models;
using Orchard.Tokens.Services;

namespace Orchard.Body.Handlers
{
    public class BodyPartHandler : ContentPartHandler<BodyPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITokenizer _tokenizer;

        public BodyPartHandler(
            IContentDefinitionManager contentDefinitionManager,
            ITokenizer tokenizer
            )
        {
            _contentDefinitionManager = contentDefinitionManager;
            _tokenizer = tokenizer;
        }

        public override void GetContentItemAspect(ContentItemAspectContext context, BodyPart part)
        {
            context.For<BodyAspect>(bodyAspect =>
            {

                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
                var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(BodyPart));
                var settings = contentTypePartDefinition.GetSettings<BodyPartSettings>();

                var body = part.Body;

                if (settings.RenderTokens && !string.IsNullOrEmpty(body))
                {
                    body = _tokenizer.Tokenize(body, new Dictionary<string, object> { ["Content"] = part.ContentItem });
                }

                bodyAspect.Body = new HtmlString(body);
            });
        }
    }
}
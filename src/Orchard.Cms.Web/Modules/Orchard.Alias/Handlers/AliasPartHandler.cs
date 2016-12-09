using System;
using System.Linq;
using Orchard.Alias.Models;
using Orchard.Alias.Settings;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Settings;
using Orchard.Tokens.Services;

namespace Orchard.Alias.Handlers
{
    public class AliasPartHandler : ContentPartHandler<AliasPart>
    {
        private readonly ITokenizer _tokenizer;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISiteService _siteService;

        public AliasPartHandler(
            ITokenizer tokenizer, 
            IContentDefinitionManager contentDefinitionManager,
            ISiteService siteService)
        {
            _tokenizer = tokenizer;
            _contentDefinitionManager = contentDefinitionManager;
            _siteService = siteService;
        }
        
        public override void Updated(UpdateContentContext context, AliasPart part)
        {
            // Compute the Path only if it's empty
            if (!String.IsNullOrEmpty(part.Alias))
            {
                return;
            }

            var pattern = GetPattern(part);

            if (!String.IsNullOrEmpty(pattern))
            {
                var ctx = _tokenizer
                    .CreateViewModel()
                    .Content(part.ContentItem);

                part.Alias = _tokenizer.Tokenize(pattern, ctx);
            }
        }
        
        /// <summary>
        /// Get the pattern from the AutoroutePartSettings property for its type
        /// </summary>
        private string GetPattern(AliasPart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, "AliasPart", StringComparison.Ordinal));
            var pattern = contentTypePartDefinition.GetSettings<AliasPartSettings>().Pattern;

            return pattern;
        }
    }
}
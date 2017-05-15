using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Alias.Models;
using Orchard.Alias.Settings;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Cache;
using Orchard.Settings;
using Orchard.Tokens.Services;

namespace Orchard.Alias.Handlers
{
    public class AliasPartHandler : ContentPartHandler<AliasPart>
    {
        private readonly ITokenizer _tokenizer;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISiteService _siteService;
        private readonly ITagCache _tagCache;

        public AliasPartHandler(
            ITokenizer tokenizer, 
            IContentDefinitionManager contentDefinitionManager,
            ISiteService siteService,
            ITagCache tagCache)
        {
            _tokenizer = tokenizer;
            _contentDefinitionManager = contentDefinitionManager;
            _siteService = siteService;
            _tagCache = tagCache;
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
                part.Alias = _tokenizer.Tokenize(pattern, new Dictionary<string, object> { ["Content"] = part.ContentItem });
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

        public override void Published(PublishContentContext context, AliasPart instance)
        {
            _tagCache.RemoveTag($"alias:{instance.Alias}");
        }

        public override void Removed(RemoveContentContext context, AliasPart instance)
        {
            _tagCache.RemoveTag($"alias:{instance.Alias}");
        }

        public override void Unpublished(PublishContentContext context, AliasPart instance)
        {
            _tagCache.RemoveTag($"alias:{instance.Alias}");
        }
    }
}
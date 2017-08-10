using System;
using System.Linq;
using Fluid;
using Orchard.Alias.Models;
using Orchard.Alias.Settings;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Cache;
using Orchard.Liquid;
using Orchard.Settings;

namespace Orchard.Alias.Handlers
{
    public class AliasPartHandler : ContentPartHandler<AliasPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISiteService _siteService;
        private readonly ITagCache _tagCache;
        private readonly ILiquidTemplateManager _liquidTemplateManager;

        public AliasPartHandler(
            IContentDefinitionManager contentDefinitionManager,
            ISiteService siteService,
            ITagCache tagCache,
            ILiquidTemplateManager liquidTemplateManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _siteService = siteService;
            _tagCache = tagCache;
            _liquidTemplateManager = liquidTemplateManager;
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
                var templateContext = new TemplateContext();
                templateContext.SetValue("ContentItem", part.ContentItem);

                part.Alias = _liquidTemplateManager.RenderAsync(pattern, templateContext).GetAwaiter().GetResult();
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
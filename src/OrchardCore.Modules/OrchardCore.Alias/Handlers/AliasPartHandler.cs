using System;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using OrchardCore.Alias.Models;
using OrchardCore.Alias.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Environment.Cache;
using OrchardCore.Liquid;
using OrchardCore.Settings;

namespace OrchardCore.Alias.Handlers
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

        public async override Task UpdatedAsync(UpdateContentContext context, AliasPart part)
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

                part.Alias = await _liquidTemplateManager.RenderAsync(pattern, templateContext);
                part.Apply();
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

        public override Task PublishedAsync(PublishContentContext context, AliasPart instance)
        {
            return _tagCache.RemoveTagAsync($"alias:{instance.Alias}");
        }

        public override Task RemovedAsync(RemoveContentContext context, AliasPart instance)
        {
            return _tagCache.RemoveTagAsync($"alias:{instance.Alias}");
        }

        public override Task UnpublishedAsync(PublishContentContext context, AliasPart instance)
        {
            return _tagCache.RemoveTagAsync($"alias:{instance.Alias}");
        }
    }
}

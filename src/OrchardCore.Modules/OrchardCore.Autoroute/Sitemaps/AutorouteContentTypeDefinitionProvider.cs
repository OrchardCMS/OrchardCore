using System.Collections.Generic;
using System.Linq;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Autoroute.Sitemaps
{
    public class AutorouteContentTypeDefinitionProvider : IRouteableContentTypeDefinitionProvider
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public AutorouteContentTypeDefinitionProvider(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public IEnumerable<ContentTypeDefinition> ListRoutableTypeDefinitions()
        {
            return _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.Parts.Any(p => p.Name == nameof(AutoroutePart)));
        }
    }
}

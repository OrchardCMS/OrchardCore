using System.Collections.Generic;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Sitemaps.Services
{
    public interface IRouteableContentTypeDefinitionProvider
    {
        IEnumerable<ContentTypeDefinition> ListRoutableTypeDefinitions();

        //TODO implement route pattern provider for use with decoupled cms.
    }
}

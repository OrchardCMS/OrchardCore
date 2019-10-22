using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services
{
    public interface IRouteableContentTypeProvider
    {
        IEnumerable<ContentTypeDefinition> ListRoutableTypeDefinitions();

        Task<string> GetRouteAsync(SitemapBuilderContext context, ContentItem contentItem);
    }
}

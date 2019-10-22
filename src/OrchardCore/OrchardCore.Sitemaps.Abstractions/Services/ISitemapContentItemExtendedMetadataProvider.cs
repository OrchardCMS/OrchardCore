using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services
{
    public interface ISitemapContentItemExtendedMetadataProvider
    {
        XAttribute GetExtendedAttribute { get; }
        Task ApplyExtendedMetadataAsync(SitemapBuilderContext context, IEnumerable<ContentItem> contentItems, ContentItem contentItem, XElement url);
    }
}

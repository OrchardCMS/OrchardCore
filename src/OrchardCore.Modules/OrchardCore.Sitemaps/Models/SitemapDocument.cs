using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.Sitemaps.Models
{
    public class SitemapDocument : Document
    {
        public IDictionary<string, SitemapType> Sitemaps { get; set; } = new Dictionary<string, SitemapType>();
    }
}

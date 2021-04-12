using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.Sitemaps.Models
{
    public interface ISitemapDocument : IDocument
    {
        public IDictionary<string, SitemapType> Sitemaps { get; set; }
    }
}

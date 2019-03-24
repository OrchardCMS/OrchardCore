using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps
{
    public interface ISitemapBuilder
    {
        Task<XDocument> BuildAsync(SitemapNode sitemapNode, SitemapBuilderContext sitemapContext);
    }
}

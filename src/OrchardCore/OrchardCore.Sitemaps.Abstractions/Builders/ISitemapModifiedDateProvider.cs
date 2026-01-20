using System;
using System.Threading.Tasks;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    /// <summary>
    /// Provides a last modified date for a sitemap.
    /// </summary>
    public interface ISitemapModifiedDateProvider
    {
        Task<DateTime?> GetLastModifiedDateAsync(SitemapType sitemap);
    }
}

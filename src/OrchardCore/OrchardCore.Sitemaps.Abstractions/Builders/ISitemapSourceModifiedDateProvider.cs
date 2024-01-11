using System;
using System.Threading.Tasks;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    /// <summary>
    /// Provides a last modified date from a sitemap source.
    /// </summary>
    public interface ISitemapSourceModifiedDateProvider
    {
        Task<DateTime?> GetLastModifiedDateAsync(SitemapSource source);
    }
}

using System;
using System.Threading.Tasks;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    /// <summary>
    /// Inherit to provide a sitemap source modified date provider.
    /// </summary>
    public abstract class SitemapSourceModifiedDateProviderBase<TSitemapSource> : ISitemapSourceModifiedDateProvider where TSitemapSource : SitemapSource
    {
        Task<DateTime?> ISitemapSourceModifiedDateProvider.GetLastModifiedDateAsync(SitemapSource source)
        {
            var tSource = source as TSitemapSource;

            if (tSource == null)
            {
                return Task.FromResult<DateTime?>(null);
            }

            return GetLastModifiedDateAsync(tSource);
        }

        /// <summary>
        /// Implement to provide a last modified date for a sitemap source.
        /// </summary>
        public virtual Task<DateTime?> GetLastModifiedDateAsync(TSitemapSource tSource)
        {
            return Task.FromResult<DateTime?>(null);
        }
    }
}

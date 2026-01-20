using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    public class DefaultSitemapModifiedDateProvider : ISitemapModifiedDateProvider
    {
        private readonly IEnumerable<ISitemapSourceModifiedDateProvider> _sitemapSourceModifiedDateProviders;

        public DefaultSitemapModifiedDateProvider(IEnumerable<ISitemapSourceModifiedDateProvider> sitemapSourceModifiedDateProviders)
        {
            _sitemapSourceModifiedDateProviders = sitemapSourceModifiedDateProviders;
        }

        public async Task<DateTime?> GetLastModifiedDateAsync(SitemapType sitemap)
        {
            DateTime? lastModifiedDate = null;
            foreach (var source in sitemap.SitemapSources)
            {
                foreach (var modifiedDateProviders in _sitemapSourceModifiedDateProviders)
                {
                    var result = await modifiedDateProviders.GetLastModifiedDateAsync(source);
                    if (result.HasValue && (lastModifiedDate == null || result.Value > lastModifiedDate))
                    {
                        lastModifiedDate = result;
                    }
                }
            }

            return lastModifiedDate;
        }
    }
}

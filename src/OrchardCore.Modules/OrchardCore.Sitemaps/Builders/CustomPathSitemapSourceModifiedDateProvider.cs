using System;
using System.Threading.Tasks;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    public class CustomPathSitemapSourceModifiedDateProvider : SitemapSourceModifiedDateProviderBase<CustomPathSitemapSource>
    {
        public override Task<DateTime?> GetLastModifiedDateAsync(CustomPathSitemapSource source)
        {
            return Task.FromResult(source.LastUpdate);
        }
    }
}

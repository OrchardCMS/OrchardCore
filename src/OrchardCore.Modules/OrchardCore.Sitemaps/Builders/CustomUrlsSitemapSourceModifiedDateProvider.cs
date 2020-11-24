using System;
using System.Threading.Tasks;
using OrchardCore.Sitemaps.Builders;
using OrchardCore.Sitemaps.Models;


namespace OrchardCore.Sitemaps.Builders
{
    public class CustomUrlsSitemapSourceModifiedDateProvider : SitemapSourceModifiedDateProviderBase<CustomUrlSitemapSource>
    {
        public override Task<DateTime?> GetLastModifiedDateAsync(CustomUrlSitemapSource source)
        {
            return Task.FromResult(source.LastUpdate);
        }
    }
}

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Cache
{
    [BackgroundTask(
        Title = "Sitemap Cache Cleaner",
        Schedule = "*/5 * * * *",
        Description = "Cleans up sitemap cache files.")]
    public class SitemapCacheBackgroundTask : IBackgroundTask
    {
        public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var sitemapManager = serviceProvider.GetRequiredService<ISitemapManager>();
            var sitemapCacheProvider = serviceProvider.GetRequiredService<ISitemapCacheProvider>();

            var sitemaps = await sitemapManager.GetSitemapsAsync();
            await sitemapCacheProvider.CleanSitemapCacheAsync(sitemaps.Select(s => s.CacheFileName));
        }
    }
}

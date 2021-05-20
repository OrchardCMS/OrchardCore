using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Handlers
{
    public class CustomPathSitemapSourceUpdateHandler : ISitemapSourceUpdateHandler
    {
        private readonly ISitemapManager _sitemapManager;

        public CustomPathSitemapSourceUpdateHandler(ISitemapManager sitemapManager)
        {
            _sitemapManager = sitemapManager;
        }

        public async Task UpdateSitemapAsync(SitemapUpdateContext context)
        {
            var sitemaps = (await _sitemapManager.LoadSitemapsAsync()).Where(s => s.GetType() == typeof(Sitemap));

            if (!sitemaps.Any())
            {
                return;
            }

            foreach (var sitemap in sitemaps)
            {
                // Do not break out of this loop, as it must check each sitemap.
                foreach (var source in sitemap.SitemapSources
                    .Select(s => s as CustomPathSitemapSource))
                {
                    if (source == null)
                    {
                        continue;
                    }

                    sitemap.Identifier = IdGenerator.GenerateId();
                }
            }

            await _sitemapManager.UpdateSitemapAsync();
        }
    }
}

using System.Threading.Tasks;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    /// <summary>
    /// Inherit to provide a sitemap source item builder.
    /// </summary>
    public abstract class SitemapSourceBuilderBase<TSitemapSource> : ISitemapSourceBuilder where TSitemapSource : SitemapSource
    {
        async Task ISitemapSourceBuilder.BuildAsync(SitemapSource source, SitemapBuilderContext context)
        {
            var tSource = source as TSitemapSource;
            if (tSource != null)
            {
                await BuildSourceAsync(tSource, context);
            }

        }

        public abstract Task BuildSourceAsync(TSitemapSource source, SitemapBuilderContext context);
    }
}

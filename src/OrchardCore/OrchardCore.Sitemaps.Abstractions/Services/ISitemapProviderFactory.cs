using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services
{
    public interface ISitemapProviderFactory
    {
        string Type { get; }
        Sitemap Create();
    }

    public class SitemapProviderFactory<TSitemap> : ISitemapProviderFactory where TSitemap : Sitemap, new()
    {
        private static readonly string TypeName = typeof(TSitemap).Name;

        private readonly ISitemapIdGenerator _sitemapIdGenerator;

        public string Type => TypeName;

        public SitemapProviderFactory(ISitemapIdGenerator sitemapIdGenerator)
        {
            _sitemapIdGenerator = sitemapIdGenerator;
        }

        public Sitemap Create()
        {
            return new TSitemap()
            {
                Id = _sitemapIdGenerator.GenerateUniqueId()
            };
        }
    }
}

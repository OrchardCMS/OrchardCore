using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services;

public interface ISitemapSourceFactory
{
    string Name { get; }
    SitemapSource Create();
}

public class SitemapSourceFactory<TSitemapSource> : ISitemapSourceFactory where TSitemapSource : SitemapSource, new()
{
    private static readonly string s_typeName = typeof(TSitemapSource).Name;

    private readonly ISitemapIdGenerator _sitemapIdGenerator;

    public SitemapSourceFactory(ISitemapIdGenerator sitemapIdGenerator)
    {
        _sitemapIdGenerator = sitemapIdGenerator;
    }

    public string Name => s_typeName;

    public SitemapSource Create()
    {
        return new TSitemapSource()
        {
            Id = _sitemapIdGenerator.GenerateUniqueId(),
        };
    }
}

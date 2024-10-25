namespace OrchardCore.Sitemaps.Cache;

/// <summary>
/// Resolves a sitemap cache file.
/// </summary>
public interface ISitemapCacheFileResolver
{
    Task<Stream> OpenReadStreamAsync();
}

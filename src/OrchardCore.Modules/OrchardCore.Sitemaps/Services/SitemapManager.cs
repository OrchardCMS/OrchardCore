using OrchardCore.Documents;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services;

public class SitemapManager : ISitemapManager
{
    private readonly IDocumentManager<SitemapDocument> _documentManager;

    public SitemapManager(IDocumentManager<SitemapDocument> documentManager)
    {
        _documentManager = documentManager;
    }

    public async Task<string> GetIdentifierAsync() => (await GetDocumentAsync()).Identifier;

    public async Task<IEnumerable<SitemapType>> LoadSitemapsAsync()
    {
        return (await LoadDocumentAsync()).Sitemaps.Values.ToArray();
    }

    public async Task<IEnumerable<SitemapType>> GetSitemapsAsync()
    {
        return (await GetDocumentAsync()).Sitemaps.Values.ToArray();
    }

    public async Task<SitemapType> LoadSitemapAsync(string sitemapId)
    {
        var document = await LoadDocumentAsync();
        if (document.Sitemaps.TryGetValue(sitemapId, out var sitemap))
        {
            return sitemap;
        }

        return null;
    }

    public async Task<SitemapType> GetSitemapAsync(string sitemapId)
    {
        var document = await GetDocumentAsync();
        if (document.Sitemaps.TryGetValue(sitemapId, out var sitemap))
        {
            return sitemap;
        }

        return null;
    }

    public async Task DeleteSitemapAsync(string sitemapId)
    {
        var existing = await LoadDocumentAsync();
        existing.Sitemaps.Remove(sitemapId);
        InvalidateIndexes(existing, sitemapId);
        await _documentManager.UpdateAsync(existing);
    }

    public async Task UpdateSitemapAsync(SitemapType sitemap)
    {
        var existing = await LoadDocumentAsync();
        existing.Sitemaps[sitemap.SitemapId] = sitemap;
        sitemap.Identifier = IdGenerator.GenerateId();
        InvalidateIndexes(existing, sitemap.SitemapId);
        await _documentManager.UpdateAsync(existing);
    }

    public async Task UpdateSitemapAsync()
    {
        var existing = await LoadDocumentAsync();
        await _documentManager.UpdateAsync(existing);
    }

    private static void InvalidateIndexes(SitemapDocument document, string sitemapId)
    {
        // Cached index XML includes the path/status/lastmod of its children.
        // A child change must invalidate that output for every existing caller.
        foreach (var index in document.Sitemaps.Values.OfType<SitemapIndex>())
        {
            if (index.SitemapSources.OfType<SitemapIndexSource>().Any(source => source.ContainedSitemapIds.Contains(sitemapId)))
            {
                index.Identifier = IdGenerator.GenerateId();
            }
        }
    }

    /// <summary>
    /// Loads the sitemap document from the store for updating and that should not be cached.
    /// </summary>
    private Task<SitemapDocument> LoadDocumentAsync() => _documentManager.GetOrCreateMutableAsync();

    /// <summary>
    /// Gets the sitemap document from the cache for sharing and that should not be updated.
    /// </summary>
    private Task<SitemapDocument> GetDocumentAsync() => _documentManager.GetOrCreateImmutableAsync();
}

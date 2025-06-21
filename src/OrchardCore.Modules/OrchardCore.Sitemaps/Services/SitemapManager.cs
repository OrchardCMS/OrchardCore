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

    public async Task<string> GetIdentifierAsync() => (await GetDocumentAsync().ConfigureAwait(false)).Identifier;

    public async Task<IEnumerable<SitemapType>> LoadSitemapsAsync()
    {
        return (await LoadDocumentAsync().ConfigureAwait(false)).Sitemaps.Values.ToArray();
    }

    public async Task<IEnumerable<SitemapType>> GetSitemapsAsync()
    {
        return (await GetDocumentAsync().ConfigureAwait(false)).Sitemaps.Values.ToArray();
    }

    public async Task<SitemapType> LoadSitemapAsync(string sitemapId)
    {
        var document = await LoadDocumentAsync().ConfigureAwait(false);
        if (document.Sitemaps.TryGetValue(sitemapId, out var sitemap))
        {
            return sitemap;
        }

        return null;
    }

    public async Task<SitemapType> GetSitemapAsync(string sitemapId)
    {
        var document = await GetDocumentAsync().ConfigureAwait(false);
        if (document.Sitemaps.TryGetValue(sitemapId, out var sitemap))
        {
            return sitemap;
        }

        return null;
    }

    public async Task DeleteSitemapAsync(string sitemapId)
    {
        var existing = await LoadDocumentAsync().ConfigureAwait(false);
        existing.Sitemaps.Remove(sitemapId);
        await _documentManager.UpdateAsync(existing).ConfigureAwait(false);
    }

    public async Task UpdateSitemapAsync(SitemapType sitemap)
    {
        var existing = await LoadDocumentAsync().ConfigureAwait(false);
        existing.Sitemaps[sitemap.SitemapId] = sitemap;
        sitemap.Identifier = IdGenerator.GenerateId();
        await _documentManager.UpdateAsync(existing).ConfigureAwait(false);
    }

    public async Task UpdateSitemapAsync()
    {
        var existing = await LoadDocumentAsync().ConfigureAwait(false);
        await _documentManager.UpdateAsync(existing).ConfigureAwait(false);
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

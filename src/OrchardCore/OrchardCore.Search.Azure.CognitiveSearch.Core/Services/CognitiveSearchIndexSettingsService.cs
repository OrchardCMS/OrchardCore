using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Search.Azure.CognitiveSearch.Models;

namespace OrchardCore.Search.Azure.CognitiveSearch.Services;

public class CognitiveSearchIndexSettingsService
{
    /// <summary>
    /// Loads the index settings document from the store for updating and that should not be cached.
    /// </summary>
    public Task<CognitiveSearchIndexSettingsDocument> LoadDocumentAsync()
        => DocumentManager.GetOrCreateMutableAsync();

    /// <summary>
    /// Gets the index settings document from the cache for sharing and that should not be updated.
    /// </summary>
    public async Task<CognitiveSearchIndexSettingsDocument> GetDocumentAsync()
    {
        var document = await DocumentManager.GetOrCreateImmutableAsync();

        foreach (var name in document.IndexSettings.Keys)
        {
            document.IndexSettings[name].IndexName = name;
        }

        return document;
    }

    public async Task<IEnumerable<CognitiveSearchIndexSettings>> GetSettingsAsync()
        => (await GetDocumentAsync()).IndexSettings.Values;

    public async Task<CognitiveSearchIndexSettings> GetSettingsAsync(string indexName)
    {
        var document = await GetDocumentAsync();

        if (document.IndexSettings.TryGetValue(indexName, out var settings))
        {
            return settings;
        }

        return null;
    }

    public async Task UpdateIndexAsync(CognitiveSearchIndexSettings settings)
    {
        var document = await LoadDocumentAsync();
        document.IndexSettings[settings.IndexName] = settings;
        await DocumentManager.UpdateAsync(document);
    }

    public async Task DeleteIndexAsync(string indexName)
    {
        var document = await LoadDocumentAsync();
        document.IndexSettings.Remove(indexName);
        await DocumentManager.UpdateAsync(document);
    }

    private static IDocumentManager<CognitiveSearchIndexSettingsDocument> DocumentManager
        => ShellScope.Services.GetRequiredService<IDocumentManager<CognitiveSearchIndexSettingsDocument>>();
}

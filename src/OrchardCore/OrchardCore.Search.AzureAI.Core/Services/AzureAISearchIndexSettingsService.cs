using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Services;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class AzureAISearchIndexSettingsService
{
    /// <summary>
    /// Loads the index settings document from the store for updating and that should not be cached.
    /// </summary>
    public Task<AzureAISearchIndexSettingsDocument> LoadDocumentAsync()
        => DocumentManager.GetOrCreateMutableAsync();

    /// <summary>
    /// Gets the index settings document from the cache for sharing and that should not be updated.
    /// </summary>
    public async Task<AzureAISearchIndexSettingsDocument> GetDocumentAsync()
    {
        var document = await DocumentManager.GetOrCreateImmutableAsync();

        foreach (var name in document.IndexSettings.Keys)
        {
            document.IndexSettings[name].IndexName = name;
        }

        return document;
    }

    public async Task<IEnumerable<AzureAISearchIndexSettings>> GetSettingsAsync()
        => (await GetDocumentAsync()).IndexSettings.Values;

    public async Task<AzureAISearchIndexSettings> GetAsync(string indexName)
    {
        var document = await GetDocumentAsync();

        if (document.IndexSettings.TryGetValue(indexName, out var settings))
        {
            return settings;
        }

        return null;
    }

    public async Task UpdateAsync(AzureAISearchIndexSettings settings)
    {
        var document = await LoadDocumentAsync();
        document.IndexSettings[settings.IndexName] = settings;
        await DocumentManager.UpdateAsync(document);
    }

    public async Task DeleteAsync(string indexName)
    {
        var document = await LoadDocumentAsync();
        document.IndexSettings.Remove(indexName);
        await DocumentManager.UpdateAsync(document);
    }

    private static IDocumentManager<AzureAISearchIndexSettingsDocument> DocumentManager
        => ShellScope.Services.GetRequiredService<IDocumentManager<AzureAISearchIndexSettingsDocument>>();
}

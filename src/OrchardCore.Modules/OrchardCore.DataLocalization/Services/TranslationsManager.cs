using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.DataLocalization.Models;
using OrchardCore.Documents;

namespace OrchardCore.DataLocalization.Services;

public class TranslationsManager : ITranslationsManager
{
    // Cache key prefix used by DataResourceManager for data localizations.
    private const string DataCultureDictionaryCacheKeyPrefix = "DataCultureDictionary-";

    private readonly IDocumentManager<TranslationsDocument> _documentManager;
    private readonly IMemoryCache _memoryCache;

    public TranslationsManager(
        IDocumentManager<TranslationsDocument> documentManager,
        IMemoryCache memoryCache)
    {
        _documentManager = documentManager;
        _memoryCache = memoryCache;
    }

    public Task<TranslationsDocument> LoadTranslationsDocumentAsync() => _documentManager.GetOrCreateMutableAsync();

    public Task<TranslationsDocument> GetTranslationsDocumentAsync() => _documentManager.GetOrCreateImmutableAsync();

    public async Task RemoveTranslationAsync(string name)
    {
        var document = await LoadTranslationsDocumentAsync();

        document.Translations.Remove(name);

        await _documentManager.UpdateAsync(document);

        // Clear the culture dictionary cache to apply the changes.
        ClearCultureDictionaryCache(name);
    }

    public async Task UpdateTranslationAsync(string name, IEnumerable<Translation> translations)
    {
        var document = await LoadTranslationsDocumentAsync();

        document.Translations[name] = translations;

        await _documentManager.UpdateAsync(document);

        // Clear the culture dictionary cache to apply the changes.
        ClearCultureDictionaryCache(name);
    }

    private void ClearCultureDictionaryCache(string cultureName)
    {
        _memoryCache.Remove(DataCultureDictionaryCacheKeyPrefix + cultureName);
    }
}

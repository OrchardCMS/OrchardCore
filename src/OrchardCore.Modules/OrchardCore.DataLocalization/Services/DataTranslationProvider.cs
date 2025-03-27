using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Localization.Data;
using OrchardCore.Localization;

namespace OrchardCore.DataLocalization.Services;

public class DataTranslationProvider : IDataTranslationProvider
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DataTranslationProvider(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <inheritdoc/>
    public void LoadTranslations(string cultureName, CultureDictionary dictionary)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var translationsManager = scope.ServiceProvider.GetService<TranslationsManager>();

            var translationsDocument = translationsManager.GetTranslationsDocumentAsync().Result;

            if (translationsDocument.Translations.TryGetValue(cultureName, out var translations))
            {
                var records = translations.Select(t => new CultureDictionaryRecord(t.Key, t.Context, [t.Value]));

                dictionary.MergeTranslations(records);
            }
        }
    }
}

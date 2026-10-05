using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DataLocalization.Models;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Localization;

namespace OrchardCore.DataLocalization.Services;

/// <summary>Overlays tenant database translations on cached PO dictionaries.</summary>
public sealed class UiTranslationProvider : ITranslationOverrideProvider
{
    private readonly ConditionalWeakTable<UiTranslationsDocument, ConditionalWeakTable<CultureDictionary, CultureDictionary>> _cache = new();

    /// <inheritdoc />
    public CultureDictionary ApplyTranslations(CultureDictionary dictionary)
    {
        var document = ShellScope.Get<UiTranslationsDocument>(typeof(UiTranslationProvider));
        if (document == null)
        {
            // DocumentManager performs distributed version checks; pin its immutable snapshot for this request.
            document = ShellScope.Services.GetRequiredService<IDocumentManager<UiTranslationsDocument>>()
                .GetOrCreateImmutableAsync().GetAwaiter().GetResult();
            ShellScope.Set(typeof(UiTranslationProvider), document);
        }

        return ApplyTranslations(dictionary, document);
    }

    /// <summary>Applies an immutable document snapshot, caching by snapshot and fallback dictionary identity.</summary>
    /// <param name="dictionary">The fallback dictionary.</param>
    /// <param name="document">The committed, immutable document snapshot.</param>
    public CultureDictionary ApplyTranslations(CultureDictionary dictionary, UiTranslationsDocument document)
        => _cache.GetValue(document, _ => new()).GetValue(dictionary, fallback =>
        {
            if (!document.Translations.TryGetValue(fallback.CultureName, out var translations) || translations.Count == 0)
            {
                return fallback;
            }

            var result = new CultureDictionary(fallback.CultureName, fallback.PluralRule);
            foreach (var translation in fallback.Translations)
            {
                result.Translations[translation.Key] = translation.Value;
            }
            result.PlainTextTranslations.UnionWith(fallback.PlainTextTranslations);
            foreach (var translation in translations)
            {
                var key = CultureDictionaryRecord.GetKey(translation.Key, string.IsNullOrEmpty(translation.Context) ? null : translation.Context);
                result.Translations[key] = translation.Values;
                result.PlainTextTranslations.Add(key);
            }

            return result;
        });
}

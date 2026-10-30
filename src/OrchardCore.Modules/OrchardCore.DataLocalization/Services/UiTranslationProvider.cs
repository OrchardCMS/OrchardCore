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
            // Keeps the plural rules of the PO records, e.g. from their Plural-Forms header.
            result.MergeTranslations(fallback.Translations.Select(t => new CultureDictionaryRecord(
                t.Key.MessageId, t.Key.Context, t.Value, fallback.GetPluralRule(t.Key))));
            result.PlainTextTranslations.UnionWith(fallback.PlainTextTranslations);

            // Overrides are edited with the plural forms of the culture, so they use its rule.
            var overrides = translations
                .Select(t => new CultureDictionaryRecord(t.Key, string.IsNullOrEmpty(t.Context) ? null : t.Context, t.Values))
                .ToArray();
            result.MergeTranslations(overrides);
            foreach (var record in overrides)
            {
                result.PlainTextTranslations.Add(record.Key);
            }

            return result;
        });
}

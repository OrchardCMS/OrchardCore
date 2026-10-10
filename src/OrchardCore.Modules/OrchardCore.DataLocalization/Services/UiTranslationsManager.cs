using System.Globalization;
using System.Text;
using OrchardCore.DataLocalization.Models;
using OrchardCore.Documents;
using OrchardCore.Localization;

namespace OrchardCore.DataLocalization.Services;

/// <summary>Manages catalog-validated UI overrides using Orchard's versioned document storage.</summary>
public sealed class UiTranslationsManager : IUiTranslationsManager
{
    private readonly IDocumentManager<UiTranslationsDocument> _documents;
    private readonly IUiLocalizationCatalog _catalog;
    private readonly ILocalizationService _localization;
    private readonly IEnumerable<IPluralRuleProvider> _pluralRules;

    /// <summary>Creates a catalog-validated tenant translation manager.</summary>
    /// <param name="documents">The tenant document manager.</param>
    /// <param name="catalog">The authoritative embedded resources.</param>
    /// <param name="localization">The site's supported cultures.</param>
    /// <param name="pluralRules">The runtime's ordered plural rules.</param>
    public UiTranslationsManager(
        IDocumentManager<UiTranslationsDocument> documents,
        IUiLocalizationCatalog catalog,
        ILocalizationService localization,
        IEnumerable<IPluralRuleProvider> pluralRules)
    {
        _documents = documents;
        _catalog = catalog;
        _localization = localization;
        _pluralRules = pluralRules.OrderBy(provider => provider.Order).ToArray();
    }

    /// <inheritdoc />
    public Task<UiTranslationsDocument> GetAsync() => _documents.GetOrCreateImmutableAsync();

    /// <inheritdoc />
    public int GetPluralFormCount(string culture)
        => Enumerable.Range(0, 1001).Max(GetPluralRule(culture).Invoke) + 1;

    /// <inheritdoc />
    public int[] GetPluralFormExamples(string culture)
    {
        var rule = GetPluralRule(culture);
        var examples = new int[GetPluralFormCount(culture)];
        Array.Fill(examples, -1);
        foreach (var count in new[] { 1, 2, 0 }.Concat(Enumerable.Range(3, 998)))
        {
            var form = rule(count);
            if (examples[form] == -1)
            {
                examples[form] = count;
            }
        }

        return examples;
    }

    private PluralizationRuleDelegate GetPluralRule(string culture)
    {
        PluralizationRuleDelegate rule = count => count == 1 ? 0 : 1;
        foreach (var provider in _pluralRules)
        {
            if (provider.TryGetRule(CultureInfo.GetCultureInfo(culture), out var providedRule))
            {
                rule = providedRule;
                break;
            }
        }

        return rule;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(string culture, IEnumerable<UiTranslation> translations)
    {
        culture = await ValidateCultureAsync(culture);
        ArgumentNullException.ThrowIfNull(translations);
        var updates = translations.ToArray();
        var resources = _catalog.GetResources().GroupBy(resource => (resource.Context, resource.Key))
            .ToDictionary(group => group.Key, group => group.First());
        var keys = new HashSet<(string Context, string Key)>();
        foreach (var update in updates)
        {
            if (update == null || update.Context == null || update.Key == null || update.Values == null ||
                !keys.Add((update.Context, update.Key)) || !resources.TryGetValue((update.Context, update.Key), out var resource))
            {
                throw new ArgumentException("Every override must identify a unique entry in the embedded catalogs.", nameof(translations));
            }

            if (!string.Equals(update.Plural, resource.Plural, StringComparison.Ordinal))
            {
                throw new ArgumentException("The plural source must match the embedded catalog.", nameof(translations));
            }

            if (update.Values.Length == 0)
            {
                continue;
            }

            var expected = resource.Plural == null ? 1 : GetPluralFormCount(culture);
            if (update.Values.Length != expected || update.Values.Any(string.IsNullOrEmpty))
            {
                throw new ArgumentException("Supply every required plural form, or remove the entire override.", nameof(translations));
            }

            if (resource.Metadata.Any(metadata => metadata.StartsWith("#,", StringComparison.Ordinal) && metadata.Contains("csharp-format", StringComparison.Ordinal)))
            {
                var argumentCount = Math.Max(CompositeFormat.Parse(resource.Key).MinimumArgumentCount,
                    resource.Plural == null ? 0 : CompositeFormat.Parse(resource.Plural).MinimumArgumentCount);
                foreach (var value in update.Values)
                {
                    if (CompositeFormat.Parse(value).MinimumArgumentCount > argumentCount)
                    {
                        throw new ArgumentException("The translation references a format argument not present in the source.", nameof(translations));
                    }
                }
            }
        }

        var document = await _documents.GetOrCreateMutableAsync();
        if (!document.Translations.TryGetValue(culture, out var existing))
        {
            existing = [];
            document.Translations[culture] = existing;
        }

        foreach (var update in updates)
        {
            existing.RemoveAll(translation => translation.Context == update.Context && translation.Key == update.Key);
            if (update.Values.Length > 0)
            {
                existing.Add(new UiTranslation
                {
                    Context = update.Context,
                    Key = update.Key,
                    Plural = update.Plural,
                    Values = update.Values.ToArray(),
                });
            }
        }

        if (existing.Count == 0)
        {
            document.Translations.Remove(culture);
        }

        await _documents.UpdateAsync(document);
    }

    /// <inheritdoc />
    public Task ImportAsync(string culture, TextReader reader)
    {
        var resources = UiPortableObject.Read(reader, "");
        if (resources.Count == 0)
        {
            throw new ArgumentException("The PO file contains no translation entries.", nameof(reader));
        }

        if (resources.Any(resource => resource.Metadata.Any(metadata => metadata.StartsWith("#,", StringComparison.Ordinal) &&
            metadata.Contains("fuzzy", StringComparison.Ordinal))))
        {
            throw new ArgumentException("Fuzzy translations must be reviewed before importing.", nameof(reader));
        }

        return UpdateAsync(culture, resources.Select(resource => new UiTranslation
        {
            Context = resource.Context,
            Key = resource.Key,
            Plural = resource.Plural,
            Values = resource.Values.All(string.IsNullOrEmpty) ? [] : resource.Values.ToArray(),
        }));
    }

    /// <inheritdoc />
    public async Task<string> ExportAsync(string culture)
    {
        culture = await ValidateCultureAsync(culture);
        var document = await GetAsync();
        return UiPortableObject.Write(culture, document.Translations.GetValueOrDefault(culture) ?? []);
    }

    private async Task<string> ValidateCultureAsync(string culture)
    {
        var cultures = await _localization.GetSupportedCulturesAsync();
        return cultures.FirstOrDefault(value => string.Equals(value, culture, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException("Select a configured supported culture.", nameof(culture));
    }
}

using System.Globalization;
using System.Text;
using OrchardCore.DataLocalization.Models;
using OrchardCore.Documents;
using OrchardCore.Localization;

namespace OrchardCore.DataLocalization.Services;

/// <summary>Manages catalog-validated UI overrides using Orchard's versioned document storage.</summary>
internal sealed class UITranslationsManager : IUITranslationsManager
{
    // Add a format here to support it for import and export.
    private static readonly IUITranslationsFormat[] _formats = [new PortableObjectFormat()];

    private readonly IDocumentManager<UITranslationsDocument> _documents;
    private readonly UILocalizationCatalog _catalog;
    private readonly ILocalizationService _localization;
    private readonly ILocalizationManager _localizationManager;

    public UITranslationsManager(
        IDocumentManager<UITranslationsDocument> documents,
        UILocalizationCatalog catalog,
        ILocalizationService localization,
        ILocalizationManager localizationManager)
    {
        _documents = documents;
        _catalog = catalog;
        _localization = localization;
        _localizationManager = localizationManager;
    }

    /// <inheritdoc />
    public IReadOnlyList<UILocalizationResource> GetResources() => _catalog.GetResources();

    /// <inheritdoc />
    public async Task<IReadOnlyList<UITranslation>> GetTranslationsAsync(string culture)
    {
        var document = await _documents.GetOrCreateImmutableAsync();
        return document.Translations.GetValueOrDefault(culture) ?? [];
    }

    /// <inheritdoc />
    public int[] GetPluralFormExamples(string culture)
    {
        var rule = _localizationManager.GetDictionary(CultureInfo.GetCultureInfo(culture)).PluralRule;
        var examples = new Dictionary<int, int>();
        foreach (var count in new[] { 1, 2, 0 }.Concat(Enumerable.Range(3, 998)))
        {
            examples.TryAdd(rule(count), count);
        }

        // Forms that no count in the range selects have no example.
        return Enumerable.Range(0, examples.Keys.Max() + 1).Select(form => examples.GetValueOrDefault(form, -1)).ToArray();
    }

    /// <inheritdoc />
    public async Task UpdateAsync(string culture, IEnumerable<UITranslation> translations)
    {
        culture = await ValidateCultureAsync(culture);
        ArgumentNullException.ThrowIfNull(translations);
        var updates = translations.ToArray();
        var resources = _catalog.GetResources().GroupBy(resource => (resource.Context, resource.Key))
            .ToDictionary(group => group.Key, group => group.First());
        var formCount = GetPluralFormExamples(culture).Length;
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

            var expected = resource.Plural == null ? 1 : formCount;
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
                existing.Add(new UITranslation
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
    public Task ImportAsync(string culture, string format, TextReader reader)
    {
        var translations = GetFormat(format).Read(reader).ToArray();
        if (translations.Length == 0)
        {
            throw new ArgumentException("The file contains no translation entries.", nameof(reader));
        }

        return UpdateAsync(culture, translations);
    }

    /// <inheritdoc />
    public async Task ExportAsync(string culture, string format, TextWriter writer)
    {
        var exporter = GetFormat(format);
        culture = await ValidateCultureAsync(culture);
        exporter.Write(writer, culture, await GetTranslationsAsync(culture));
    }

    private static IUITranslationsFormat GetFormat(string format)
        => _formats.FirstOrDefault(value => string.Equals(value.Extension, format, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException("The file format is not supported.", nameof(format));

    private async Task<string> ValidateCultureAsync(string culture)
    {
        var cultures = await _localization.GetSupportedCulturesAsync();
        return cultures.FirstOrDefault(value => string.Equals(value, culture, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException("Select a configured supported culture.", nameof(culture));
    }
}

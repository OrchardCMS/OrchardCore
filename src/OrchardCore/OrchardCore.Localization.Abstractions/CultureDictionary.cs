using System.Collections;
using System.Globalization;

namespace OrchardCore.Localization;

/// <summary>
/// Represents a dictionary for a certain culture.
/// </summary>
/// <remarks>This type is not thread safe.</remarks>
public class CultureDictionary : IEnumerable<CultureDictionaryRecord>
{
    private readonly Dictionary<CultureDictionaryRecordKey, PluralizationRuleDelegate> _pluralRules = [];

    /// <summary>
    /// Creates a new instance of <see cref="CultureDictionary"/>.
    /// </summary>
    /// <param name="cultureName">The culture name.</param>
    /// <param name="pluralRule">The pluralization rule.</param>
    public CultureDictionary(string cultureName, PluralizationRuleDelegate pluralRule)
    {
        Translations = new Dictionary<CultureDictionaryRecordKey, string[]>();
        CultureName = cultureName;
        PluralRule = pluralRule;
    }

    /// <summary>
    /// Gets the culture name.
    /// </summary>
    public string CultureName { get; }

    /// <summary>
    /// Gets the pluralization rule of the culture, used for the translations that don't define their own rule.
    /// </summary>
    public PluralizationRuleDelegate PluralRule { get; }

    /// <summary>
    /// Gets the localized value.
    /// </summary>
    /// <param name="key">The resource key.</param>
    /// <returns></returns>
    public string this[CultureDictionaryRecordKey key] => this[key, null];

    /// <summary>
    /// Gets the localized value.
    /// </summary>
    /// <param name="key">The resource key.</param>
    /// <param name="count">The number to specify the pluralization form.</param>
    /// <returns></returns>
    public string this[CultureDictionaryRecordKey key, int? count]
    {
        get
        {
            if (!Translations.TryGetValue(key, out var translations))
            {
                return null;
            }

            var pluralForm = count.HasValue ? GetPluralRule(key)(count.Value) : 0;
            if (pluralForm >= translations.Length)
            {
                throw new PluralFormNotFoundException($"Plural form '{pluralForm}' doesn't exist for the key '{key}' in the '{CultureName}' culture.", new PluralForm(key, pluralForm, CultureInfo.GetCultureInfo(CultureName)));
            }

            return translations[pluralForm];
        }
    }

    /// <summary>
    /// Gets a list of the culture translations including the plural forms.
    /// </summary>
    public IDictionary<CultureDictionaryRecordKey, string[]> Translations { get; }

    /// <summary>
    /// Gets the pluralization rule selecting the plural form of the translations of a resource key.
    /// </summary>
    /// <param name="key">The resource key.</param>
    /// <returns>
    /// The rule of the record merged for the <paramref name="key"/>, e.g. the one defined by the <c>Plural-Forms</c> header
    /// of its PO file, otherwise the <see cref="PluralRule"/> of the culture.
    /// </returns>
    public PluralizationRuleDelegate GetPluralRule(CultureDictionaryRecordKey key)
        => _pluralRules.TryGetValue(key, out var pluralRule) ? pluralRule : PluralRule;

    /// <summary>
    /// Merges the translations from multiple dictionary records.
    /// </summary>
    /// <param name="records">The records to be merged.</param>
    /// <remarks>
    /// The <see cref="CultureDictionaryRecord.PluralRule"/> of a record, if any, replaces the <see cref="PluralRule"/>
    /// of the culture for its translations.
    /// </remarks>
    public void MergeTranslations(IEnumerable<CultureDictionaryRecord> records)
    {
        foreach (var record in records)
        {
            Translations[record.Key] = record.Translations;

            if (record.PluralRule != null)
            {
                _pluralRules[record.Key] = record.PluralRule;
            }
            else
            {
                _pluralRules.Remove(record.Key);
            }
        }
    }

    public IEnumerator<CultureDictionaryRecord> GetEnumerator()
    {
        foreach (var item in Translations)
        {
            _pluralRules.TryGetValue(item.Key, out var pluralRule);

            yield return new CultureDictionaryRecord(item.Key, null, item.Value, pluralRule);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

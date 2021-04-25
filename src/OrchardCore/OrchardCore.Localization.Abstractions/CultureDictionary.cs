using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Represents a dictionary for a certain culture.
    /// </summary>
    /// <remarks>This type is not thread safe.</remarks>
    public class CultureDictionary : IEnumerable<CultureDictionaryRecord>
    {
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
        /// gets the pluralization rule.
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

                var pluralForm = count.HasValue ? PluralRule(count.Value) : 0;
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
        /// Merges the translations from multiple dictionary records.
        /// </summary>
        /// <param name="records">The records to be merged.</param>
        public void MergeTranslations(IEnumerable<CultureDictionaryRecord> records)
        {
            foreach (var record in records)
            {
                Translations[record.Key] = record.Translations;
            }
        }

        public IEnumerator<CultureDictionaryRecord> GetEnumerator()
        {
            foreach (var item in Translations)
            {
                yield return new CultureDictionaryRecord(item.Key, null, item.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

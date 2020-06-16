using System;
using System.Collections.Generic;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Represents a dictionary for a certain culture.
    /// </summary>
    public class CultureDictionary
    {
        /// <summary>
        /// Creates a new instance of <see cref="CultureDictionary"/>.
        /// </summary>
        /// <param name="cultureName">The culture name.</param>
        /// <param name="pluralRule">The pluralization rule.</param>
        public CultureDictionary(string cultureName, PluralizationRuleDelegate pluralRule)
        {
            Translations = new Dictionary<string, string[]>();
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
        /// <param name="count">The number to specify the pluralization form.</param>
        /// <returns></returns>
        public string this[string key, int? count = null]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (!Translations.TryGetValue(key, out string[] translations))
                {
                    return null;
                }

                var pluralForm = count.HasValue ? PluralRule(count.Value) : 0;
                if (pluralForm >= translations.Length)
                {
                    throw new PluralFormNotFoundException($"Plural form '{pluralForm}' doesn't exist for the key '{key}' in the '{CultureName}' culture.");
                }

                return translations[pluralForm];
            }
        }

        /// <summary>
        /// Gets a list of the culture translations including the plural forms.
        /// </summary>
        public IDictionary<string, string[]> Translations { get; private set; }

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
    }
}

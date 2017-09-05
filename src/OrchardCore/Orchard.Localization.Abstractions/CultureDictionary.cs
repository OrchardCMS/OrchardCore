using System;
using System.Collections.Generic;

namespace OrchardCore.Localization
{
    public class CultureDictionary
    {
        public string CultureName { get; private set; }

        public PluralizationRuleDelegate PluralRule { get; private set; }

        public string this[string key] => this[key, null];

        public string this[string key, int? count]
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

        public IDictionary<string, string[]> Translations { get; private set; }

        public CultureDictionary(string cultureName, PluralizationRuleDelegate pluralRule)
        {
            Translations = new Dictionary<string, string[]>();
            CultureName = cultureName;
            PluralRule = pluralRule;
        }

        public void MergeTranslations(IEnumerable<CultureDictionaryRecord> records)
        {
            foreach (var record in records)
            {
                Translations[record.Key] = record.Translations;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Orchard.Localization.Abstractions
{
    public class CultureDictionary
    {
        private IDictionary<string, string[]> _translations;

        public string CultureName { get; private set; }
        public Func<int, int> PluralRule { get; private set; }
        public string[] this[string key] => _translations.ContainsKey(key) ? _translations[key] : null;

        public CultureDictionary(string cultureName, Func<int, int> pluralRule)
        {
            _translations = new Dictionary<string, string[]>();
            CultureName = cultureName;
            PluralRule = pluralRule;
        }

        public void MergeTranslations(IEnumerable<CultureDictionaryRecord> records)
        {
            foreach (var record in records)
            {
                _translations[record.Key] = record.Translations;
            }
        }
    }
}

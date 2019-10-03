using System.Collections.Generic;

namespace OrchardCore.Localization.DynamicData
{
    public class DynamicDataTranslationsProvider : ITranslationProvider
    {
        public void LoadTranslations(string cultureName, CultureDictionary dictionary)
        {
            // TODO: Load the translation from the database
            var records = new List<CultureDictionaryRecord>();

            switch(cultureName)
            {
                case "ar":
                    records.Add(new CultureDictionaryRecord("Hello", null, new string[] { "مرحباً" }));
                    records.Add(new CultureDictionaryRecord("Bye", null, new string[] { "وداعاً" }));
                    break;
                case "fr":
                    records.Add(new CultureDictionaryRecord("Hello", null, new string[] { "Bonjour" }));
                    records.Add(new CultureDictionaryRecord("Bye", null, new string[] { "au revoir" }));
                    break;
            }

            dictionary.MergeTranslations(records);
        }
    }
}

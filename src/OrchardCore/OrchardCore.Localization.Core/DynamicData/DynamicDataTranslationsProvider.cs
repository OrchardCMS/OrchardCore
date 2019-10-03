using System.Linq;

namespace OrchardCore.Localization.DynamicData
{
    public class DynamicDataTranslationsProvider : ITranslationProvider
    {
        public void LoadTranslations(string cultureName, CultureDictionary dictionary)
        {
            // TODO: Load the translation from the database
            var records = Enumerable.Empty<CultureDictionaryRecord>();
            dictionary.MergeTranslations(records);
        }
    }
}

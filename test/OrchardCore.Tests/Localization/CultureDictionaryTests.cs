using OrchardCore.Localization;
using Xunit;

namespace OrchardCore.Tests.Localization
{
    public class CultureDictionaryTests
    {
        private static PluralizationRuleDelegate _csPluralRule = n => ((n == 1) ? 0 : (n >= 2 && n <= 4) ? 1 : 2);

        [Fact]
        public void MergeAddsRecordToEmptyDictionary()
        {
            var dictionary = new CultureDictionary("cs", _csPluralRule);
            var record = new CultureDictionaryRecord("ball", "míč", "míče", "míčů");

            dictionary.MergeTranslations(new[] { record });

            Assert.Equal(dictionary.Translations[record.Key], record.Translations);
        }

        [Fact]
        public void MergeOverwritesTranslationsForSameKeys()
        {
            var dictionary = new CultureDictionary("cs", _csPluralRule);
            var record = new CultureDictionaryRecord("ball","míč", "míče", "míčů");
            var record2 = new CultureDictionaryRecord("ball", "balón", "balóny", "balónů");

            dictionary.MergeTranslations(new[] { record });
            dictionary.MergeTranslations(new[] { record2 });

            Assert.Equal(dictionary.Translations[record.Key], record2.Translations);
        }

        [Fact]
        public void IndexerReturnNullIfKeyDoesntExist()
        {
            var dictionary = new CultureDictionary("cs", _csPluralRule);

            var translation = dictionary["ball"];

            Assert.Null(translation);
        }

        [Fact]
        public void IntexerThrowsPluralFormNotFoundExceptionIfSpecifiedPluralFormDoesntExist()
        {
            var dictionary = new CultureDictionary("cs", _csPluralRule);
            var record = new CultureDictionaryRecord("ball", "míč", "míče");
            dictionary.MergeTranslations(new[] { record });

            Assert.Throws<PluralFormNotFoundException>(() => dictionary["ball", 5]);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using OrchardCore.Localization;
using Xunit;

namespace OrchardCore.Tests.Localization
{
    public class CultureDictionaryTests
    {
        private static PluralizationRuleDelegate _arPluralRule = n => (n == 0 ? 0 : n == 1 ? 1 : n == 2 ? 2 : n % 100 >= 3 && n % 100 <= 10 ? 3 : n % 100 >= 11 ? 4 : 5);
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
            var record = new CultureDictionaryRecord("ball", "míč", "míče", "míčů");
            var record2 = new CultureDictionaryRecord("ball", "balón", "balóny", "balónů");

            dictionary.MergeTranslations(new[] { record });
            dictionary.MergeTranslations(new[] { record2 });

            Assert.Equal(dictionary.Translations[record.Key], record2.Translations);
        }

        [Fact]
        public void IndexerReturnNullIfKeyDoesntExist()
        {
            var dictionary = new CultureDictionary("cs", _csPluralRule);
            var key = new CultureDictionaryRecordKey("ball");
            var translation = dictionary[key];

            Assert.Null(translation);
        }

        [Fact]
        public void IndexerThrowsPluralFormNotFoundExceptionIfSpecifiedPluralFormDoesntExist()
        {
            // Arrange
            var dictionary = new CultureDictionary("cs", _csPluralRule);
            var record = new CultureDictionaryRecord("ball", "míč", "míče");
            dictionary.MergeTranslations(new[] { record });

            Assert.Throws<PluralFormNotFoundException>(() =>
            {
                var key = new CultureDictionaryRecordKey("ball");

                return dictionary[key, 5];
            });
        }

        [Fact]
        public void EnumerateCultureDictionary()
        {
            // Arrange
            var dictionary = new CultureDictionary("ar", _arPluralRule);
            dictionary.MergeTranslations(new List<CultureDictionaryRecord>
            {
                new CultureDictionaryRecord("Hello", "مرحبا"),
                new CultureDictionaryRecord("Bye", "مع السلامة")
            });

            // Act & Assert
            Assert.NotEmpty(dictionary);

            foreach (var record in dictionary)
            {
                Assert.NotNull(record.Key);
                Assert.Single(record.Translations);
            }

            Assert.Equal(2, dictionary.Count());
        }
    }
}

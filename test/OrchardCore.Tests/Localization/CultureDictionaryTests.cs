using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization
{
    public class CultureDictionaryTests
    {
        [Fact]
        public void MergeAddsRecordToEmptyDictionary()
        {
            var dictionary = new CultureDictionary("cs", PluralizationRule.Czech);
            var record = new CultureDictionaryRecord("ball", "míč", "míče", "míčů");

            dictionary.MergeTranslations(new[] { record });

            Assert.Equal(dictionary.Translations[record.Key], record.Translations);
        }

        [Fact]
        public void MergeOverwritesTranslationsForSameKeys()
        {
            var dictionary = new CultureDictionary("cs", PluralizationRule.Czech);
            var record = new CultureDictionaryRecord("ball", "míč", "míče", "míčů");
            var record2 = new CultureDictionaryRecord("ball", "balón", "balóny", "balónů");

            dictionary.MergeTranslations(new[] { record });
            dictionary.MergeTranslations(new[] { record2 });

            Assert.Equal(dictionary.Translations[record.Key], record2.Translations);
        }

        [Fact]
        public void IndexerReturnNullIfKeyDoesntExist()
        {
            var dictionary = new CultureDictionary("cs", PluralizationRule.Czech);
            var key = new CultureDictionaryRecordKey("ball");
            var translation = dictionary[key];

            Assert.Null(translation);
        }

        [Fact]
        public void IndexerThrowsPluralFormNotFoundExceptionIfSpecifiedPluralFormDoesntExist()
        {
            // Arrange
            var dictionary = new CultureDictionary("cs", PluralizationRule.Czech);
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
            var dictionary = new CultureDictionary("ar", PluralizationRule.Arabic);
            dictionary.MergeTranslations(new List<CultureDictionaryRecord>
            {
                new CultureDictionaryRecord("Hello", "مرحبا"),
                new CultureDictionaryRecord("Bye", "مع السلامة")
            });

            // Act & Assert
            Assert.NotEmpty(dictionary);

            foreach (var record in dictionary)
            {
                Assert.Single(record.Translations);
            }

            Assert.Equal(2, dictionary.Count());
        }
    }
}

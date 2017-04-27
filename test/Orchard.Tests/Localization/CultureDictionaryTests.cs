using Orchard.Localization;
using Orchard.Localization.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Orchard.Tests.Localization
{
    public class CultureDictionaryTests
    {
        private static Func<int, int> _csPluralRule = n => ((n == 1) ? 0 : (n >= 2 && n <= 4) ? 1 : 2);

        [Fact]
        public void MergeAddsRecordToEmptyDictionary()
        {
            var dictionary = new CultureDictionary("cs", _csPluralRule);
            var record = new CultureDictionaryRecord("ball", null, new[] { "míč", "míče", "míčů" });

            dictionary.MergeTranslations(new[] { record });

            Assert.Equal(dictionary[record.Key], record.Translations);
        }

        [Fact]
        public void MergeOverwritesTranslationsForSameKeys()
        {
            var dictionary = new CultureDictionary("cs", _csPluralRule);
            var record = new CultureDictionaryRecord("ball", null, new[] { "míč", "míče", "míčů" });
            var record2 = new CultureDictionaryRecord("ball", null, new[] { "balón", "balóny", "balónů" });

            dictionary.MergeTranslations(new[] { record });
            dictionary.MergeTranslations(new[] { record2 });

            Assert.Equal(dictionary[record.Key], record2.Translations);
        }

        [Fact]
        public void IndexerReturnNullIfKeyDoesntExist()
        {
            var dictionary = new CultureDictionary("cs", _csPluralRule);

            var translation = dictionary["ball"];

            Assert.Null(translation);
        }
    }
}

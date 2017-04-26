using Microsoft.Extensions.Caching.Memory;
using Moq;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Orchard.Tests.Localization
{
    public class LocalizationManagerTests
    {
        private Func<int, int> _csPluralRule = n => ((n == 1) ? 0 : (n >= 2 && n <= 4) ? 1 : 2);
        private Mock<IPluralRuleProvider> _pluralRuleProvider;
        private Mock<ITranslationProvider> _translationProvider;
        private IMemoryCache _memoryCache;

        public LocalizationManagerTests()
        {
            _pluralRuleProvider = new Mock<IPluralRuleProvider>();
            _pluralRuleProvider.Setup(o => o.GetRule(It.Is<string>(culture => culture == "cs"))).Returns(_csPluralRule);

            _translationProvider = new Mock<ITranslationProvider>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
        }

        [Fact]
        public void GetDictionaryReturnsDictionaryWithPluralRuleAndCultureIfNoTranslationsExists()
        {
            _translationProvider.Setup(o => o.LoadTranslationsToDictionary(
                It.Is<string>(culture => culture == "cs"),
                It.IsAny<CultureDictionary>())
            );
            var manager = new LocalizationManager(_pluralRuleProvider.Object, _translationProvider.Object, _memoryCache);

            var dictionary = manager.GetDictionary("cs");

            Assert.Equal("cs", dictionary.CultureName);
            Assert.Equal(_csPluralRule, dictionary.PluralRule);
        }

        [Fact]
        public void GetDictionaryReturnsDictionaryWithTranslationsFromProvider()
        {
            var dictionaryRecord = new CultureDictionaryRecord("ball", null, new[] { "míč", "míče", "míčů" });
            _translationProvider
                .Setup(o => o.LoadTranslationsToDictionary(It.Is<string>(culture => culture == "cs"), It.IsAny<CultureDictionary>()))
                .Callback<string, CultureDictionary>((culture, dictioanry) => dictioanry.MergeTranslations(new[] { dictionaryRecord }));
            var manager = new LocalizationManager(_pluralRuleProvider.Object, _translationProvider.Object, _memoryCache);

            var dictionary = manager.GetDictionary("cs");

            Assert.Equal(dictionary["ball"], dictionaryRecord.Translations);
        }
    }
}

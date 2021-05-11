using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using OrchardCore.Localization;
using Xunit;

namespace OrchardCore.Tests.Localization
{
    public class LocalizationManagerTests
    {
        private PluralizationRuleDelegate _csPluralRule = n => ((n == 1) ? 0 : (n >= 2 && n <= 4) ? 1 : 2);
        private Mock<IPluralRuleProvider> _pluralRuleProvider;
        private Mock<ITranslationProvider> _translationProvider;
        private IMemoryCache _memoryCache;

        public LocalizationManagerTests()
        {
            _pluralRuleProvider = new Mock<IPluralRuleProvider>();
            _pluralRuleProvider.SetupGet(o => o.Order).Returns(0);
            _pluralRuleProvider.Setup(o => o.TryGetRule(It.Is<CultureInfo>(culture => culture.Name == "cs"), out _csPluralRule)).Returns(true);

            _translationProvider = new Mock<ITranslationProvider>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
        }

        [Fact]
        public void GetDictionaryReturnsDictionaryWithPluralRuleAndCultureIfNoTranslationsExists()
        {
            _translationProvider.Setup(o => o.LoadTranslations(
                It.Is<string>(culture => culture == "cs"),
                It.IsAny<CultureDictionary>())
            );
            var manager = new LocalizationManager(new[] { _pluralRuleProvider.Object }, new[] { _translationProvider.Object }, _memoryCache);

            var dictionary = manager.GetDictionary(new CultureInfo("cs"));

            Assert.Equal("cs", dictionary.CultureName);
            Assert.Equal(_csPluralRule, dictionary.PluralRule);
        }

        [Fact]
        public void GetDictionaryReturnsDictionaryWithTranslationsFromProvider()
        {
            var dictionaryRecord = new CultureDictionaryRecord("ball", "míč", "míče", "míčů");
            _translationProvider
                .Setup(o => o.LoadTranslations(It.Is<string>(culture => culture == "cs"), It.IsAny<CultureDictionary>()))
                .Callback<string, CultureDictionary>((culture, dictioanry) => dictioanry.MergeTranslations(new[] { dictionaryRecord }));
            var manager = new LocalizationManager(new[] { _pluralRuleProvider.Object }, new[] { _translationProvider.Object }, _memoryCache);

            var dictionary = manager.GetDictionary(new CultureInfo("cs"));
            var key = new CultureDictionaryRecordKey("ball");

            dictionary.Translations.TryGetValue(key, out var translations);

            Assert.Equal(translations, dictionaryRecord.Translations);
        }

        [Fact]
        public void GetDictionarySelectsPluralRuleFromProviderWithHigherPriority()
        {
            PluralizationRuleDelegate csPluralRuleOverride = n => ((n == 1) ? 0 : (n >= 2 && n <= 4) ? 1 : 0);

            var highPriorityRuleProvider = new Mock<IPluralRuleProvider>();
            highPriorityRuleProvider.SetupGet(o => o.Order).Returns(-1);
            highPriorityRuleProvider.Setup(o => o.TryGetRule(It.Is<CultureInfo>(culture => culture.Name == "cs"), out csPluralRuleOverride)).Returns(true);

            _translationProvider.Setup(o => o.LoadTranslations(
                It.Is<string>(culture => culture == "cs"),
                It.IsAny<CultureDictionary>())
            );
            var manager = new LocalizationManager(new[] { _pluralRuleProvider.Object, highPriorityRuleProvider.Object }, new[] { _translationProvider.Object }, _memoryCache);

            var dictionary = manager.GetDictionary(new CultureInfo("cs"));

            Assert.Equal(dictionary.PluralRule, csPluralRuleOverride);
        }
    }
}

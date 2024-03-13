using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization
{
    public class LocalizationManagerTests
    {
        private readonly Mock<IPluralRuleProvider> _pluralRuleProvider;
        private readonly Mock<ITranslationProvider> _translationProvider;
        private readonly IMemoryCache _memoryCache;

        public LocalizationManagerTests()
        {
            var csPluralRule = PluralizationRule.Czech;
            _pluralRuleProvider = new Mock<IPluralRuleProvider>();
            _pluralRuleProvider.SetupGet(o => o.Order).Returns(0);
            _pluralRuleProvider.Setup(o => o.TryGetRule(It.Is<CultureInfo>(culture => culture.Name == "cs"), out csPluralRule)).Returns(true);

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
            Assert.Equal(PluralizationRule.Czech, dictionary.PluralRule);
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

using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization
{
    public class DefaultPluralRuleProviderTests
    {
        [Theory]
        [InlineData("en-US", "en")]
        [InlineData("zh-TW", "zh")]
        [InlineData("zh-HanT-TW", "zh")]
        [InlineData("zh-CN", "zh")]
        [InlineData("zh-Hans-CN", "zh")]
        [InlineData("zh-Hans", "zh")]
        [InlineData("zh-Hant", "zh")]
        public void TryGetRuleShouldReturnRuleForTopCulture(string culture, string expected)
        {
            var expectedCulture = CultureInfo.GetCultureInfo(expected);
            var testCulture = CultureInfo.GetCultureInfo(culture);

            IPluralRuleProvider pluralProvider = new DefaultPluralRuleProvider();
            pluralProvider.TryGetRule(expectedCulture, out var expectedPlural);
            pluralProvider.TryGetRule(testCulture, out var testPlural);

            Assert.NotNull(expectedPlural);
            Assert.NotNull(testPlural);
            Assert.Same(expectedPlural, testPlural);
        }
    }
}

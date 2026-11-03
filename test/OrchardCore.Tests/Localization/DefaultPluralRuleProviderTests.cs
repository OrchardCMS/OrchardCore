using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization;

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
    [InlineData("fr-CA", "fr")]
    [InlineData("es-MX", "es")]
    [InlineData("sr-Latn-RS", "sr")]
    [InlineData("kok-Latn-IN", "kok-Latn")]
    [InlineData("pt-Latn-AO", "pt")]
    public void TryGetRule_UnregisteredCulture_ReturnsRuleForParentCulture(string culture, string expected)
    {
        var expectedCulture = CultureInfo.GetCultureInfo(expected);
        var testCulture = CultureInfo.GetCultureInfo(culture);

        var pluralProvider = new DefaultPluralRuleProvider();
        Assert.True(pluralProvider.TryGetRule(expectedCulture, out var expectedPlural));
        Assert.True(pluralProvider.TryGetRule(testCulture, out var testPlural));

        Assert.NotNull(expectedPlural);
        Assert.NotNull(testPlural);
        Assert.Same(expectedPlural, testPlural);
    }

    [Theory]
    [InlineData("pt", 0, 0)]
    [InlineData("pt", 1, 0)]
    [InlineData("pt", 2, 1)]
    [InlineData("pt", 10, 1)]
    [InlineData("pt", 999_999, 1)]
    [InlineData("pt", 1_000_000, 2)]
    [InlineData("pt", 1_000_001, 1)]
    [InlineData("pt", 2_000_000, 2)]
    [InlineData("pt", -1, 0)]
    [InlineData("pt", -1_000_000, 2)]
    [InlineData("pt", int.MinValue, 1)]
    [InlineData("pt", int.MaxValue, 1)]
    [InlineData("pt-BR", 0, 0)]
    [InlineData("pt-BR", 1, 0)]
    [InlineData("pt-BR", 2, 1)]
    [InlineData("pt-BR", 10, 1)]
    [InlineData("pt-BR", 999_999, 1)]
    [InlineData("pt-BR", 1_000_000, 2)]
    [InlineData("pt-BR", 1_000_001, 1)]
    [InlineData("pt-BR", 2_000_000, 2)]
    [InlineData("pt-BR", -1, 0)]
    [InlineData("pt-BR", -1_000_000, 2)]
    [InlineData("pt-BR", int.MinValue, 1)]
    [InlineData("pt-BR", int.MaxValue, 1)]
    [InlineData("pt-PT", 0, 1)]
    [InlineData("pt-PT", 1, 0)]
    [InlineData("pt-PT", 2, 1)]
    [InlineData("pt-PT", 10, 1)]
    [InlineData("pt-PT", 999_999, 1)]
    [InlineData("pt-PT", 1_000_000, 2)]
    [InlineData("pt-PT", 1_000_001, 1)]
    [InlineData("pt-PT", 2_000_000, 2)]
    [InlineData("pt-PT", -1, 0)]
    [InlineData("pt-PT", -1_000_000, 2)]
    [InlineData("pt-PT", int.MinValue, 1)]
    [InlineData("pt-PT", int.MaxValue, 1)]
    public void TryGetRule_PortugueseCulture_ReturnsExpectedPluralForm(string culture, int count, int expected)
    {
        var pluralProvider = new DefaultPluralRuleProvider();

        Assert.True(pluralProvider.TryGetRule(CultureInfo.GetCultureInfo(culture), out var rule));
        Assert.NotNull(rule);
        Assert.Equal(expected, rule(count));
    }

    [Theory]
    [InlineData("pt-AO")]
    [InlineData("pt-CH")]
    [InlineData("pt-CV")]
    [InlineData("pt-FR")]
    [InlineData("pt-GQ")]
    [InlineData("pt-GW")]
    [InlineData("pt-LU")]
    [InlineData("pt-MO")]
    [InlineData("pt-MZ")]
    [InlineData("pt-ST")]
    [InlineData("pt-TL")]
    public void TryGetRule_PortugueseLocale_IgnoresMainComponentParentOverrides(string culture)
    {
        var pluralProvider = new DefaultPluralRuleProvider();

        Assert.True(pluralProvider.TryGetRule(CultureInfo.GetCultureInfo("pt"), out var expectedRule));
        Assert.True(pluralProvider.TryGetRule(CultureInfo.GetCultureInfo(culture), out var rule));
        Assert.Same(expectedRule, rule);
        Assert.Equal(0, rule(0));
        Assert.Equal(0, rule(1));
        Assert.Equal(1, rule(2));
        Assert.Equal(2, rule(1_000_000));
    }

    [Theory]
    [InlineData("pt", "one")]
    [InlineData("pt-BR", "one")]
    [InlineData("pt-PT", "other")]
    public void TryGetRule_PortugueseDictionary_SelectsMatchingTranslation(string culture, string zeroTranslation)
    {
        var pluralProvider = new DefaultPluralRuleProvider();
        Assert.True(pluralProvider.TryGetRule(CultureInfo.GetCultureInfo(culture), out var rule));

        var dictionary = new CultureDictionary(culture, rule);
        var key = new CultureDictionaryRecordKey { MessageId = "item" };
        dictionary.Translations[key] = ["one", "other", "many"];

        Assert.Equal(zeroTranslation, dictionary[key, 0]);
        Assert.Equal("one", dictionary[key, 1]);
        Assert.Equal("other", dictionary[key, 2]);
        Assert.Equal("many", dictionary[key, 1_000_000]);
        Assert.Equal("other", dictionary[key, 1_000_001]);
    }

    [Theory]
    [InlineData("fa", 0, 0)]
    [InlineData("fa", 2, 1)]
    [InlineData("vi", 2, 1)]
    [InlineData("cgg", 0, 1)]
    [InlineData("ka", 1, 0)]
    [InlineData("as", 0, 0)]
    [InlineData("tr", 0, 1)]
    [InlineData("uz", 0, 1)]
    [InlineData("jv", 1, 0)]
    [InlineData("yo", 1, 0)]
    [InlineData("yo", 2, 0)]
    [InlineData("fil", 2, 0)]
    [InlineData("fil", 4, 1)]
    [InlineData("fil", 10, 0)]
    [InlineData("mk", 11, 1)]
    [InlineData("mk", 111, 1)]
    [InlineData("lt", 5, 1)]
    [InlineData("lt", 19, 2)]
    [InlineData("lv", 10, 2)]
    [InlineData("lv", 19, 2)]
    [InlineData("he", 2, 2)]
    [InlineData("sat", 2, 2)]
    [InlineData("se", 2, 2)]
    [InlineData("fr", 1_000_000, 2)]
    [InlineData("gl", 2_000_000, 2)]
    [InlineData("br", 0, 1)]
    [InlineData("br", 2, 2)]
    [InlineData("br", 3, 3)]
    [InlineData("br", 71, 1)]
    [InlineData("br", 1_000_000, 4)]
    [InlineData("cy", 0, 4)]
    [InlineData("cy", 3, 2)]
    [InlineData("cy", 6, 5)]
    [InlineData("cy", 8, 3)]
    [InlineData("kw", 0, 4)]
    [InlineData("kw", 21, 5)]
    [InlineData("kw", 22, 1)]
    [InlineData("kw", 23, 2)]
    [InlineData("kw", 1000, 1)]
    [InlineData("kw", 21_000, 3)]
    [InlineData("kw", 100_000, 1)]
    [InlineData("mt", 2, 4)]
    [InlineData("mt", 102, 3)]
    [InlineData("sl", 101, 1)]
    [InlineData("en", -1, 0)]
    [InlineData("ru", -21, 0)]
    [InlineData("ar", -2, 2)]
    [InlineData("en", int.MinValue, 1)]
    [InlineData("ru", int.MinValue, 2)]
    [InlineData("ach", 0, 0)]
    [InlineData("csb", 22, 1)]
    [InlineData("mnk", 0, 0)]
    public void TryGetRule_IntegerCardinalRegression_ReturnsExpectedIndex(string culture, int count, int expected)
    {
        var provider = new DefaultPluralRuleProvider();
        Assert.True(provider.TryGetRule(CultureInfo.GetCultureInfo(culture), out var rule));

        Assert.Equal(expected, rule(count));
    }

    [Theory]
    [InlineData("")]
    [InlineData("zxx")]
    public void TryGetRule_UnsupportedCulture_ReturnsFalseAndNullRule(string culture)
    {
        var pluralProvider = new DefaultPluralRuleProvider();

        Assert.False(pluralProvider.TryGetRule(CultureInfo.GetCultureInfo(culture), out var rule));
        Assert.Null(rule);
    }
}

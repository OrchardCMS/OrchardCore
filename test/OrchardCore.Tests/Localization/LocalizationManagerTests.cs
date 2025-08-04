using System.Text.Json.Nodes;
using OrchardCore.Localization;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Localization;

public class LocalizationManagerTests : IDisposable
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

        var dictionary = manager.GetDictionary(CultureInfo.GetCultureInfo("cs"));

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

        var manager = new LocalizationManager([_pluralRuleProvider.Object], [_translationProvider.Object], _memoryCache);

        var dictionary = manager.GetDictionary(CultureInfo.GetCultureInfo("cs"));
        var key = new CultureDictionaryRecordKey { MessageId = "ball" };

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

        var manager = new LocalizationManager([_pluralRuleProvider.Object, highPriorityRuleProvider.Object], [_translationProvider.Object], _memoryCache);

        var dictionary = manager.GetDictionary(CultureInfo.GetCultureInfo("cs"));

        Assert.Equal(dictionary.PluralRule, csPluralRuleOverride);
    }

    [Theory]
    [InlineData("en", "Hello en !")]
    [InlineData("zh-CN", "你好！")]
    public async Task TestLocalizationRule(string culture, string expected)
    {
        var context = new SiteContext();
        await context.InitializeAsync();

        var recipeSteps = new JsonArray
        {
            new JsonObject
            {
                {"name", "Feature"},
                {
                    "enable", new JsonArray
                    {
                        "OrchardCore.Localization",
                        "OrchardCore.Localization.ContentLanguageHeader",
                    }
                },
            },
            new JsonObject
            {
                {"name", "Settings"},
                {"LocalizationSettings", new JsonObject
                    {
                        {"DefaultCulture", culture},
                        {"SupportedCultures", new JsonArray(culture) },
                    }
                },
            },
        };

        var recipe = new JsonObject
        {
            {"steps", recipeSteps},
        };

        await RecipeHelpers.RunRecipeAsync(context, recipe);

        await context.UsingTenantScopeAsync(scope =>
        {
            using var cultureScope = CultureScope.Create(culture, culture);
            var localizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<LocalizationManagerTests>>();

            // Assert
            Assert.Equal(expected, localizer["hello!"]);

            return Task.CompletedTask;
        });
    }

    public void Dispose() => _memoryCache?.Dispose();
}

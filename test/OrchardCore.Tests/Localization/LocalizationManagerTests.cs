using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Localization;
using OrchardCore.Localization.Models;
using OrchardCore.Settings;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Localization;

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

        await context.UsingTenantScopeAsync(async scope =>
        {
            var shellFeaturesManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var availableFeatures = await shellFeaturesManager.GetAvailableFeaturesAsync();
            var featureIds = new string[] { "OrchardCore.Localization.ContentLanguageHeader", "OrchardCore.Localization" };
            var features = availableFeatures.Where(feature => featureIds.Contains(feature.Id));

            await shellFeaturesManager.EnableFeaturesAsync(features, true);

            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var siteSettings = await siteService.LoadSiteSettingsAsync();

            siteSettings.Alter<LocalizationSettings>("LocalizationSettings", localizationSettings =>
            {
                localizationSettings.DefaultCulture = culture;
                localizationSettings.SupportedCultures = [culture];
            });

            await siteService.UpdateSiteSettingsAsync(siteSettings);

            var shellSettings = scope.ServiceProvider.GetRequiredService<ShellSettings>();
            var shellHost = scope.ServiceProvider.GetRequiredService<IShellHost>();

            await shellHost.ReleaseShellContextAsync(shellSettings);
        });

        await context.UsingTenantScopeAsync(scope =>
        {
            using var cultureScope = CultureScope.Create(culture, culture);
            var localizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<LocalizationManagerTests>>();

            // Assert
            Assert.Equal(expected, localizer["hello!"]);

            return Task.CompletedTask;
        });
    }
}

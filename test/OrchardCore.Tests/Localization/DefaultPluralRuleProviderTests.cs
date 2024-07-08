using OrchardCore.Environment.Shell;
using OrchardCore.Features.Services;
using OrchardCore.Localization;
using OrchardCore.Tests.Apis.Context;

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

            var pluralProvider = new DefaultPluralRuleProvider();
            pluralProvider.TryGetRule(expectedCulture, out var expectedPlural);
            pluralProvider.TryGetRule(testCulture, out var testPlural);

            Assert.NotNull(expectedPlural);
            Assert.NotNull(testPlural);
            Assert.Same(expectedPlural, testPlural);
        }

        [Fact]
        public async Task TestSayHello()
        {
            var context = new SiteContext();
            await context.InitializeAsync();
            await context.UsingTenantScopeAsync(async scope =>
             {
                 var shellFeaturesManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
                 var availableFeatures = await shellFeaturesManager.GetAvailableFeaturesAsync();
                 var demoFeature = availableFeatures.FirstOrDefault(feature => feature.Id == "OrchardCore.Demo");

                 await shellFeaturesManager.EnableFeaturesAsync([demoFeature], true);

                 var shellSettings = scope.ServiceProvider.GetRequiredService<ShellSettings>();
                 var shellHost = scope.ServiceProvider.GetRequiredService<IShellHost>();
                 await shellHost.ReloadShellContextAsync(shellSettings);
             });

            var response = await context.Client.GetAsync("api/demo/sayhello");

            response.EnsureSuccessStatusCode();
        }
    }
}

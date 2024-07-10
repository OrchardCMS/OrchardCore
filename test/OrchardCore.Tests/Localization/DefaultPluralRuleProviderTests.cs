using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Localization;
using OrchardCore.Localization.Models;
using OrchardCore.Settings;
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
                 var featureIds = new string[] { "OrchardCore.Localization.ContentLanguageHeader", "OrchardCore.Localization", "OrchardCore.Demo" };
                 var features = availableFeatures.Where(feature => featureIds.Contains(feature.Id));

                 await shellFeaturesManager.EnableFeaturesAsync(features, true);

                 var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
                 var siteSettings = await siteService.LoadSiteSettingsAsync();
                 siteSettings.Alter<LocalizationSettings>("LocalizationSettings", localizationSettings =>
                 {
                     localizationSettings.DefaultCulture = "en";
                     localizationSettings.SupportedCultures = ["en", "zh-CN"];
                 });
                 await siteService.UpdateSiteSettingsAsync(siteSettings);

                 var shellSettings = scope.ServiceProvider.GetRequiredService<ShellSettings>();
                 var shellHost = scope.ServiceProvider.GetRequiredService<IShellHost>();
                 await shellHost.ReleaseShellContextAsync(shellSettings);
             });

            // /Localization/en/OrchardCore.Demo.po
            var requestEn = new HttpRequestMessage(HttpMethod.Get, "api/demo/SayHello");
            var content2 = new StringContent("");
            content2.Headers.ContentLanguage.Add("en");
            requestEn.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("zh-CN"));
            requestEn.Content = content2;
            var response2 = await context.Client.SendAsync(requestEn);

            response2.EnsureSuccessStatusCode();

            var result2 = await response2.Content.ReadAsStringAsync();  
            Assert.Equal("Hello en!", result2);

            // /Localization/zh-CN/OrchardCore.Demo.po
            var request = new HttpRequestMessage(HttpMethod.Get, "api/demo/SayHello");
            var content = new StringContent("");
            content.Headers.ContentLanguage.Add("zh-CN");
            request.Content = content;
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("zh-CN"));
            var response = await context.Client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal("你好！", result); 
        }
    }
}

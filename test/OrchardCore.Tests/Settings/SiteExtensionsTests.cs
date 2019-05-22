using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using OrchardCore.Settings;
using Xunit;
using OrchardCoreLocalization = OrchardCore.Localization;

namespace OrchardCore.Tests.Settings
{
    public class SiteExtensionsTests
    {
        private const string DefaultCulture = "en-US";

        private Mock<ISite> _site;

        public SiteExtensionsTests()
        {
            _site = new Mock<ISite>();
        }

        [Theory]
        [InlineData(null, null, new string[] { "en-US" })]
        [InlineData(null, new string[] { "ar", "fr" }, new string[] { "en-US", "ar", "fr" })]
        [InlineData("ar", new string[] { "ar", "fr" }, new string[] { "ar", "fr" })]
        public void SiteReturnGetConfiguredCultures(string defaultCulture, string[] supportedCultures, string[] expected)
        {
            SetupSiteSettingsCultures(defaultCulture, supportedCultures);

            var configuredCultures = SiteExtensions.GetConfiguredCultures(_site.Object);

            Assert.Equal(expected, configuredCultures);
        }

        [Fact]
        public void SiteReturnGetConfiguredCulturesWithInvariantCultures()
        {
            SetupSiteSettingsCultures(CultureInfo.InstalledUICulture.Name, new string[] { "ar", "fr" });

            var configuredCultures = SiteExtensions.GetConfiguredCultures(_site.Object);

            Assert.Equal(new string[] { CultureInfo.InstalledUICulture.Name, "ar", "fr" }, configuredCultures);
        }

        [Fact]
        public void SiteReturnGetConfiguredCulturesContainsInvariantCultureIfDefaultCultureIsNull()
        {
            SetupSiteSettingsCultures(null, new string[] { "ar", "fr" });

            var configuredCultures = SiteExtensions.GetConfiguredCultures(_site.Object);

            Assert.Equal(new string[] { CultureInfo.InstalledUICulture.Name, "ar", "fr" }, configuredCultures);
        }

        [Theory]
        [InlineData(null, null, DefaultCulture, new string[] { DefaultCulture })]
        [InlineData("", null, DefaultCulture, new string[] { DefaultCulture })]
        [InlineData(null, new string[] { }, DefaultCulture, new string[] { DefaultCulture })]
        [InlineData(null, new string[] { "ar", "fr" }, DefaultCulture, new string[] { "ar", "fr" })]
        [InlineData("ar", new string[] { "ar", "fr" }, "ar", new string[] { "ar", "fr" })]
        public Task SiteReturnGetConfiguredCulturesWithLocalizationMiddleware(string defaultCulture, string[] supportedCultures, string expectedDefaultCulture, string[] expectedSupportedCultures)
            => RunTest(defaultCulture, supportedCultures, expectedDefaultCulture, expectedSupportedCultures);

        [Fact]
        public Task SiteReturnGetConfiguredCulturesWithLocalizationMiddlewareAndInvariantCulture()
            => RunTest(CultureInfo.InstalledUICulture.Name, new string[] { }, CultureInfo.InstalledUICulture.Name, new[] { CultureInfo.InstalledUICulture.Name });

        private async Task RunTest(string defaultCulture, string[] supportedCultures, string expectedDefaultCulture, string[] expectedSupportedCultures)
        {
            var localizationStartup = new OrchardCoreLocalization.Startup();
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    SetupSiteSettingsCultures(defaultCulture, supportedCultures);
                    services.AddTransient<ISiteService>(_ => new SiteService(_site.Object));

                    services.AddRouting();

                    localizationStartup.ConfigureServices(services);
                })
                .Configure(app =>
                {
                    localizationStartup.Configure(app, new RouteBuilder(app), app.ApplicationServices);

                    app.Run(context =>
                    {
                        var requestLocalizationOptions = context.RequestServices.GetService<IOptions<RequestLocalizationOptions>>().Value;
                        var defaultRequestCulture = requestLocalizationOptions.DefaultRequestCulture;

                        Assert.Equal(expectedDefaultCulture, requestLocalizationOptions.DefaultRequestCulture.Culture.Name);
                        Assert.Equal(expectedSupportedCultures, requestLocalizationOptions.SupportedCultures.Select(c => c.Name).ToArray());

                        return Task.FromResult(0);
                    });
                });

            using (var server = new TestServer(webHostBuilder))
            {
                var client = server.CreateClient();
                var requestedCulture = "en";
                var response = await client.GetAsync($"page/?culture={requestedCulture}&ui-culture={requestedCulture}");
            }
        }

        private void SetupSiteSettingsCultures(string defaultCulture, string[] supportedCultures)
        {
            _site.Setup(s => s.Culture).Returns(defaultCulture);
            _site.Setup(s => s.SupportedCultures).Returns(supportedCultures);
        }

        private class SiteService : ISiteService
        {
            private readonly ISite _site;

            public SiteService(ISite site)
            {
                _site = site;
            }

            public IChangeToken ChangeToken => throw new NotImplementedException();

            public Task<ISite> GetSiteSettingsAsync() => Task.FromResult(_site);

            public Task UpdateSiteSettingsAsync(ISite site)
            {
                throw new NotImplementedException();
            }
        }
    }
}
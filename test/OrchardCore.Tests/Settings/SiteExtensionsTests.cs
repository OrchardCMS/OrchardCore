using Moq;
using OrchardCore.Settings;
using Xunit;

namespace OrchardCore.Tests.Settings
{
    public class SiteExtensionsTests
    {
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

        private void SetupSiteSettingsCultures(string defaultCulture, string[] supportedCultures)
        {
            _site.Setup(s => s.Culture).Returns(defaultCulture);
            _site.Setup(s=> s.SupportedCultures).Returns(supportedCultures);
        }
    }
}
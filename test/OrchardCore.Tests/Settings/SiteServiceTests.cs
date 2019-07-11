using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Fields;
using OrchardCore.CustomSettings;
using OrchardCore.ContentManagement;
using OrchardCore.Settings;
using Xunit;


namespace OrchardCore.Tests.Settings
{
    public class SiteServiceTests
    {
        [Fact]
        public async Task ShouldReturnCustomSettings()
        {
            var testValue = "test";
            var contentItem = new ContentItem();
            contentItem.GetOrCreate<TestSettings>();
            contentItem.Alter<TestSettings>(part =>
            {
                part.Alter<TextField>("TestSetting", field => field.Text = testValue);
                part.TestSetting = part.Get<TextField>("TestSetting");
            });

            var mockSiteService = Mock.Of<ISiteService>(ss =>
                ss.GetSiteSettingsAsync() == Task.FromResult(
                    Mock.Of<ISite>(s => s.Properties == JObject.FromObject(new { TestSettings = contentItem }))
                )
            );

            var allSettingsResult = await mockSiteService.GetSiteSettingsAsync();
            var testSettingsResult = await mockSiteService.GetCustomSettingsAsync<TestSettings>();

            Assert.IsType<TestSettings>(testSettingsResult);
            Assert.NotNull(testSettingsResult);
            Assert.Equal(testValue, testSettingsResult.TestSetting?.Text);
        }
    }
}

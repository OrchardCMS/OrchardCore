using OrchardCore.Settings;
using OrchardCore.Sms.Azure.Models;
using OrchardCore.Tests.Utilities;

namespace OrchardCore.Sms.Azure.Services.Tests;

public class AzureSmsProviderTests
{
    [Fact(Skip = "Configure the default sender and connection string for SMS Communication Services (ACS) before run this test.")]
    public async Task SendSmsShouldSucceed()
    {
        // Arrange
        var siteSettings = GetSiteService();
        var azureSmsProvider = new AzureSmsProvider(
            siteSettings,
            Mock.Of<IDataProtectionProvider>(),
            Mock.Of<ILogger<AzureSmsProvider>>(),
            Mock.Of<IStringLocalizer<AzureSmsProvider>>());
        var message = new SmsMessage
        {
            To = "test@orchardcore.net",
            Body = "This is a test message."
        };

        // Act
        var result = await azureSmsProvider.SendAsync(message);

        // Assert
        Assert.True(result.Succeeded);
    }

    private static ISiteService GetSiteService()
    {
        var azureSmsSettings = new AzureSmsSettings
        {
            PhoneNumber = "<<Sender>>",
            ConnectionString = "<<ConnectionString>>"
        };
        var siteServiceMock = new Mock<ISiteService>();

        siteServiceMock.
            Setup(siteService => siteService.GetSiteSettingsAsync()).
            ReturnsAsync(SiteMockHelper.GetSite(azureSmsSettings).Object);

        return siteServiceMock.Object;
    }
}

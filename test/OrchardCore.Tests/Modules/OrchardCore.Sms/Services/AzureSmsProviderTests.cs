using OrchardCore.Sms.Azure.Models;

namespace OrchardCore.Sms.Azure.Services.Tests;

public class AzureSmsProviderTests
{
    [Fact(Skip = "Configure the default sender and connection string for SMS Communication Services (ACS) before run this test.")]
    public async Task SendSmsShouldSucceed()
    {
        // Arrange
        var azureSmsOptions = new AzureSmsOptions
        {
            IsEnabled = true,
            PhoneNumber = "<<Sender>>",
            ConnectionString = "<<ConnectionString>>"
        };
        var azureSmsProvider = new AzureSmsProvider(
            Options.Create(azureSmsOptions),
            Mock.Of<IPhoneFormatValidator>(),
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
}

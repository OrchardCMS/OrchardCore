namespace OrchardCore.Email.Azure.Services.Tests;

public class AzureEmailDeliveryServiceTests
{
    [Fact(Skip = "Configure the default sender and connection string for Email Communication Services (ECS) before run this test.")]
    public async Task SendEmailShouldSucceed()
    {
        // Arrange
        var emailOptions = Options.Create(new AzureEmailSettings
        {
            DefaultSender = "<<Sender>>",
            ConnectionString = "<<ConnectionString>>"
        });
        var emailDeliveryService = new AzureEmailDeliveryService(
            emailOptions,
            Mock.Of<NullLogger<AzureEmailDeliveryService>>(),
            Mock.Of<IStringLocalizer<AzureEmailDeliveryService>>());
        var message = new MailMessage
        {
            To = "test@orchardcore.net",
            Subject = "Orchard Core",
            Body = "This is a test message."
        };

        // Act
        var result = await emailDeliveryService.DeliverAsync(message);

        // Assert
        Assert.True(result.Succeeded);
    }
}

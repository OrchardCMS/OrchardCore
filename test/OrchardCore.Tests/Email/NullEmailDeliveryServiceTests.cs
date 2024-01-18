using OrchardCore.Email.Services;

namespace OrchardCore.Email.Tests;

public class NullEmailDeliveryServiceTests
{
    [Fact]
    public async Task SendEmail()
    {
        // Arrange
        var emailDeliveryService = new NullEmailDeliveryService();
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
